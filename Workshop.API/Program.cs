using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Events;
using Workshop.Core;
using Workshop.Core.Interfaces.IRepositories;
using Workshop.Core.Interfaces.IServices;
using Workshop.Core.Logging;
using Workshop.Core.Security;
using Workshop.Core.Services;
using Workshop.Infrastructure;
using Workshop.Infrastructure.Contexts;
using Workshop.Infrastructure.Repositories;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("System", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .Enrich.WithProperty("Application", "Workshop.API")
    .WriteTo.Console()
    .WriteTo.File("logs/bootstrap-.log", rollingInterval: RollingInterval.Day)
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // ------------------- Serilog (database primary, file fallback) -------------------
    var connectionString = builder.Configuration.GetConnectionString("Default")!;

    // ── Build the DapperLogSink early (needs DapperContext directly) ──────────
    var dapperContext = new DapperContext(builder.Configuration);
    var logRepo = new LogRepository(dapperContext);
    var logWriter = new LogWriterService(logRepo);

    builder.Host.UseSerilog((ctx, services, config) => config
        .ReadFrom.Configuration(ctx.Configuration)
        .ReadFrom.Services(services)
        .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
        .MinimumLevel.Override("System", LogEventLevel.Warning)
        .Enrich.FromLogContext()
        .Enrich.WithMachineName()
        .Enrich.WithProperty("Application", "Workshop.API")
        .WriteTo.Console()
        .WriteTo.WithDapperSink(logWriter));

    // ------------------- Database/Contexts -------------------
    builder.Services.AddDbContext<WorkshopDbContext>(options =>
        options.UseSqlServer(connectionString));

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

    // ------------------- Middleware -------------------
    app.UseSerilogRequestLogging(options =>
    {
        options.MessageTemplate =
            "HTTP {RequestMethod} {RequestPath} -> {StatusCode} in {Elapsed:0.0000}ms TraceId={TraceIdentifier}";
        options.EnrichDiagnosticContext = (diag, http) =>
        {
            diag.Set("TraceIdentifier", http.TraceIdentifier);
        };
    });

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
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}