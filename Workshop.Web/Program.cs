using Hangfire;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Localization;
using Microsoft.IdentityModel.Tokens;
using System.Globalization;
using System.Text;
using Workshop.Resources;
using Workshop.Web.Extensions;
using Workshop.Web.Interfaces.Services;
using Workshop.Web.Models;
using Workshop.Web.Services;
var builder = WebApplication.CreateBuilder(args);


 
builder.Services.AddHangfire(buil => buil
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseSqlServerStorage(builder.Configuration.GetConnectionString("Default"))
);
builder.Services.AddHangfireServer();  // immediately after AddHangfire
builder.Services.AddControllersWithViews();

// Add Resource Services
builder.Services.AddResourceServices();

var cultureInfo = new CultureInfo("en-CA"); // en-CA يعتمد yyyy-MM-dd
CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

// Add services to the container.
builder.Services.AddControllersWithViews();

// Add API settings configuration
builder.Services.AddTransient<IApiAuthStrategy, OldApiAuthStrategy>();

// Add all HTTP clients
builder.Services.AddHttpClients(builder.Configuration);

// Add Helpers
builder.Services.AddScoped<IFileService, FileService>();
builder.Services.AddScoped<IFileValidationService, FileValidationService>();

// Add Resource Services
builder.Services.AddResourceServices();
builder.Services.AddMemoryCache();


//builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
//    .AddJwtBearer(options =>
//    {
//        options.TokenValidationParameters = new TokenValidationParameters
//        {
//            ValidateIssuerSigningKey = true,
//            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])),
//            ValidateIssuer = true,
//            ValidIssuer = builder.Configuration["Jwt:Issuer"],
//            ValidateAudience = true,
//            ValidAudience = builder.Configuration["Jwt:Audience"],
//            ValidateLifetime = true,
//            ClockSkew = TimeSpan.Zero // Optional: Reduce or eliminate clock skew tolerance
//        };
//    });
builder.Services.AddAuthorization();

// Configure supported cultures
var supportedCultures = new[]
{
    new CultureInfo("en"),
    new CultureInfo("ar")
};

builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    options.DefaultRequestCulture = new RequestCulture("en");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
});
builder.Services.AddDistributedMemoryCache();
builder.Services.AddHttpContextAccessor();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(100);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add<SessionTimeout>();
});

var app = builder.Build();
var accessor = app.Services.GetRequiredService<IHttpContextAccessor>();
PermissionHelper.Configure(accessor);

// ----------- GLOBAL EXCEPTION LOGGING -------------
app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (Exception ex)
    {
        var logger = context.RequestServices
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger("GlobalException");

        logger.LogError(ex,
            "Unhandled MVC exception. TraceId={TraceId} Path={Path}",
            context.TraceIdentifier,
            context.Request.Path);

        throw;
    }
});

// MVC ERROR PAGE
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseSession();
app.UseStaticFiles();

// Add localization middleware
app.UseRequestLocalization();

app.UseRouting();
app.UseSession();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Authentication}/{action=Index}/{id?}");
app.UseHangfireDashboard("/hangfire");


// ------------------- Run -------------------
try
{
    app.Run();
}
catch (Exception ex)
{
    var logger = app.Services
        .GetRequiredService<ILoggerFactory>()
        .CreateLogger("Host");

    logger.LogCritical(ex, "MVC host terminated unexpectedly");
    throw;
}