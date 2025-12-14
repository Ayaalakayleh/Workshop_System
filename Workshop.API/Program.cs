using Microsoft.EntityFrameworkCore;
using Workshop.Core.Interfaces.IRepositories;
using Workshop.Core.Interfaces.IServices;
using Workshop.Core.Security;
using Workshop.Core.Services;
using Workshop.Core;
using Workshop.Infrastructure;
using Workshop.Infrastructure.Contexts;
using Workshop.Infrastructure.Repositories;


var builder = WebApplication.CreateBuilder(args);
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

