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
    options.IdleTimeout = TimeSpan.FromMinutes(40);
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
// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios.
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
app.Run();
