using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Workshop.Core;
using Workshop.Core.Interfaces.IRepositories;
using Workshop.Core.Interfaces.IServices;
using Workshop.Core.Security;
using Workshop.Core.Services;
using Workshop.Infrastructure;
using Workshop.Infrastructure.Contexts;
using Workshop.Infrastructure.Repositories;


var builder = WebApplication.CreateBuilder(args);

// ------------------- Logging -------------------
builder.Logging.ClearProviders();
builder.Logging.AddConsole(o => o.IncludeScopes = true);
builder.Logging.AddDebug();

// ------------------- Database/Contexts -------------------
builder.Services.AddDbContext<WorkshopDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Default")));


// Add controller
builder.Services.AddControllers();

builder.Services.AddInfrastructure();
builder.Services.AddCoreServices();

builder.Services.AddSingleton<DapperContext>();
builder.Services.AddSingleton<SecurityHelper>();



builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyHeader();
        policy.AllowAnyMethod();
        policy.AllowAnyOrigin();
    });
});
var app = builder.Build();

// global exception logging - by Mahmoud
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var feature = context.Features.Get<IExceptionHandlerFeature>();
        var ex = feature?.Error;

        var logger = context.RequestServices
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger("GlobalException");

        logger.LogError(ex,
            "Unhandled exception. TraceId={TraceId} Path={Path}",
            context.TraceIdentifier,
            context.Request.Path);

        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/problem+json";

        await context.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Title = "An unexpected error occurred.",
            Status = StatusCodes.Status500InternalServerError,
            Instance = context.TraceIdentifier
        });
    });
});

// request logging - by Mahmoud
app.Use(async (context, next) =>
{
    var sw = System.Diagnostics.Stopwatch.StartNew();
    try
    {
        await next();
    }
    finally
    {
        sw.Stop();

        var logger = context.RequestServices
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger("Request");

        logger.LogInformation(
            "HTTP {Method} {Path} -> {StatusCode} in {ElapsedMs}ms TraceId={TraceId}",
            context.Request.Method,
            context.Request.Path,
            context.Response.StatusCode,
            sw.ElapsedMilliseconds,
            context.TraceIdentifier);
    }
});

// Swagger only in development
if (true || app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// DO NOT force HTTPS here — IIS handles it
// app.UseHttpsRedirection();  REMOVE this in IIS-hosted deployments

app.UseRouting();
app.UseAuthorization();
app.MapControllers();
app.UseCors(policy =>
{
    policy.AllowAnyHeader();
    policy.AllowAnyMethod();
    policy.AllowAnyOrigin();
});
app.Run();

