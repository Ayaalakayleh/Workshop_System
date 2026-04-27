using Hangfire;
using Microsoft.AspNetCore.Localization;
using Serilog;
using Serilog.Events;
using System.Globalization;
using Workshop.Core.Logging;
using Workshop.Resources;
using Workshop.Web.Extensions;
using Workshop.Web.Interfaces.Services;
using Workshop.Web.Models;
using Workshop.Web.Services;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("System", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .Enrich.WithProperty("Application", "Workshop.Web")
    .WriteTo.Console()
    .WriteTo.File("logs/bootstrap-.log", rollingInterval: RollingInterval.Day)
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // ------------------- Serilog → Workshop.API HTTP sink -------------------
    var apiBaseUrl = builder.Configuration["ApiSettings:BaseApiUrl"]
                     ?? throw new InvalidOperationException("ApiSettings:BaseApiUrl is not configured.");

    builder.Host.UseSerilog((ctx, services, config) => config
        .ReadFrom.Configuration(ctx.Configuration)
        .ReadFrom.Services(services)
        .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
        .MinimumLevel.Override("System", LogEventLevel.Warning)
        .Enrich.FromLogContext()
        .Enrich.WithMachineName()
        .Enrich.WithProperty("Application", "Workshop.Web")
        .WriteTo.Console()
        .WriteTo.WithApiFallback(apiBaseUrl));

    // ------------------- Hangfire -------------------
    builder.Services.AddHangfire(buil => buil
        .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
        .UseSimpleAssemblyNameTypeSerializer()
        .UseRecommendedSerializerSettings()
        .UseSqlServerStorage(builder.Configuration.GetConnectionString("Default"))
    );
    builder.Services.AddHangfireServer();

    // ------------------- MVC -------------------
    builder.Services.AddControllersWithViews(options =>
    {
        options.Filters.Add<SessionTimeout>();
    });

    // ------------------- Resources & HTTP -------------------
    builder.Services.AddResourceServices();

    var cultureInfo = new CultureInfo("en-CA");
    CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
    CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

    builder.Services.AddTransient<IApiAuthStrategy, OldApiAuthStrategy>();
    builder.Services.AddHttpClients(builder.Configuration);

    // ------------------- Helpers & Services -------------------
    builder.Services.AddScoped<IFileService, FileService>();
    builder.Services.AddScoped<IFileValidationService, FileValidationService>();
    builder.Services.AddScoped<WorkflowEmailService>();
    builder.Services.AddScoped<EmailSender>();
    builder.Services.AddMemoryCache();
    builder.Services.AddAuthorization();

    // ------------------- Localization -------------------
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

        options.RequestCultureProviders = new List<IRequestCultureProvider>
        {
            new CustomRequestCultureProvider(context =>
            {
                var lang = context.Request.Cookies["Language"];

                if (!string.IsNullOrWhiteSpace(lang))
                {
                    lang = lang.Trim().ToLower();

                    if (lang == "ar" || lang == "en")
                    {
                        return Task.FromResult<ProviderCultureResult?>(
                            new ProviderCultureResult(lang, lang));
                    }
                }

                return Task.FromResult<ProviderCultureResult?>(null);
            }),

            new QueryStringRequestCultureProvider(),
            new CookieRequestCultureProvider(),
            new AcceptLanguageHeaderRequestCultureProvider()
        };
    });

    // ------------------- Session -------------------
    builder.Services.AddDistributedMemoryCache();
    builder.Services.AddHttpContextAccessor();

    builder.Services.AddSession(options =>
    {
        options.IdleTimeout = TimeSpan.FromMinutes(100);
        options.Cookie.HttpOnly = true;
        options.Cookie.IsEssential = true;
        options.Cookie.Name = ".Workshop.Session";
    });

    var app = builder.Build();

    var accessor = app.Services.GetRequiredService<IHttpContextAccessor>();
    PermissionHelper.Configure(accessor);

    // ------------------- Middleware -------------------
    app.UseSerilogRequestLogging(options =>
    {
        options.MessageTemplate =
            "HTTP {RequestMethod} {RequestPath} -> {StatusCode} in {Elapsed:0.0000}ms TraceId={TraceIdentifier}";
        options.EnrichDiagnosticContext = (diag, http) =>
            diag.Set("TraceIdentifier", http.TraceIdentifier);
    });

    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Home/Error");
        app.UseHsts();
    }

    app.UseStaticFiles();

    app.UseRouting();
    app.UseSession();

    var locOptions = app.Services
        .GetRequiredService<Microsoft.Extensions.Options.IOptions<RequestLocalizationOptions>>()
        .Value;

    app.UseRequestLocalization(locOptions);

    app.UseAuthorization();

    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Authentication}/{action=Index}/{id?}");

    app.UseHangfireDashboard("/hangfire");

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Web host terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}