using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using AspNetCoreRateLimit;
using LMS.API.Middlewares;
using LMS.API.Services;
using LMS.API.Swagger;
using LMS.Application;
using LMS.Application.Common.Converters;
using LMS.Application.Common.Interfaces;
using LMS.Application.Common.Models;
using LMS.Application.Common.Options;
using LMS.Application.Localization;
using LMS.Infrastructure;
using LMS.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using NET_Tracker.Controllers;
using NET_Tracker.Extensions;
using Serilog;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Globalization;
using System.Reflection;

namespace LMS.API
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            try
            {
                await RunApp(args);
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "LMS API failed to start.");
                TryWriteStartupError(ex);
                throw;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        private static void TryWriteStartupError(Exception ex)
        {
            try
            {
                var logsPath = Path.Combine(AppContext.BaseDirectory, "Logs");
                Directory.CreateDirectory(logsPath);
                File.AppendAllText(Path.Combine(logsPath, "startup-error.txt"),
                    $"{DateTime.UtcNow:O} {ex}\n\n");
            }
            catch
            {
            }
        }

        private static async Task RunApp(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var env = builder.Environment;

            Directory.SetCurrentDirectory(env.ContentRootPath);
            var logsPath = Path.Combine(env.ContentRootPath, "Logs");
            if (!Directory.Exists(logsPath))
            {
                Directory.CreateDirectory(logsPath);
            }

            Serilog.Debugging.SelfLog.Enable(msg =>
            {
                try
                {
                    File.AppendAllText(Path.Combine(logsPath, "serilog-selflog.txt"), $"{DateTime.UtcNow:O} {msg}\n");
                }
                catch { }
            });

            builder.Configuration.Sources.Clear();
            builder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true);

            JsonLocalizationProvider.Initialize(env.ContentRootPath);

            if (env.IsDevelopment() || env.EnvironmentName == "Test")
            {
                var appAssembly = Assembly.Load(new AssemblyName(env.ApplicationName));
                if (appAssembly != null) builder.Configuration.AddUserSecrets(appAssembly, optional: true);
            }

            var logFilePath = Path.Combine(logsPath, "log-.txt");

            builder.Configuration.AddEnvironmentVariables().AddCommandLine(args);
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
                .MinimumLevel.Override("System", Serilog.Events.LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .WriteTo.File(
                    path: logFilePath,
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 31,
                    shared: true,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
                )
                .ReadFrom.Configuration(builder.Configuration)
                .CreateBootstrapLogger();

            builder.Host.UseSerilog((context, services, configuration) => configuration
                .MinimumLevel.Information()
                .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
                .MinimumLevel.Override("System", Serilog.Events.LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .WriteTo.File(
                    path: logFilePath,
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 31,
                    shared: true,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
                )
                .ReadFrom.Configuration(context.Configuration)
                .ReadFrom.Services(services));

            builder.Services.AddApplication(builder.Configuration);
            builder.Services.AddInfrastructure(builder.Configuration);
            builder.Services.AddPersistence(builder.Configuration);

            var netTrackerConn = builder.Configuration.GetConnectionString("LMSConnectionString");
            if (string.IsNullOrWhiteSpace(netTrackerConn))
            {
                netTrackerConn = builder.Configuration.GetConnectionString("DefaultConnection");
            }

            var isNetTrackerEnabled = builder.Configuration.GetValue<bool?>("NetTracker:Enabled") ?? true;
            var hasNetTracker = isNetTrackerEnabled && !string.IsNullOrWhiteSpace(netTrackerConn);

            if (hasNetTracker)
            {
                builder.Configuration["ConnectionStrings:DefaultConnection"] ??= netTrackerConn;
                builder.Configuration["NetTracker:Storage:ConnectionString"] ??= netTrackerConn;
                builder.Configuration["NetTracker:Enabled"] = "true";
                builder.Configuration["NetTracker:EnableDashboardUI"] = "true";
                builder.Configuration["NetTracker:AllowRemoteDashboardAccess"] = "true";


                builder.Services.AddNetTracker(builder.Configuration);
                builder.Services.PostConfigure<NET_Tracker.Configuration.HttpLoggingOptions>(options =>
                {
                    options.Enabled = true;
                    options.EnableDashboardUI = true;
                    options.AllowRemoteDashboardAccess = true;
                    options.MaxBodySize = 8192;
                    options.ExcludePaths ??= new List<string>();
                    var exclusions = new[]
                    {
                        "/health",
                        "/swagger",
                        "/swagger-custom",
                        "/favicon.ico",
                        "/net-tracker",
                        "/api/Statistics",
                        "/api/HttpTransactions",
                        "/files",
                    };

                    foreach (var path in exclusions)
                    {
                        if (!options.ExcludePaths.Contains(path, StringComparer.OrdinalIgnoreCase))
                        {
                            options.ExcludePaths.Add(path);
                        }
                    }

                    if (options.Retention != null)
                    {
                        options.Retention.DaysToKeep = 7;
                        options.Retention.AutoCleanup = true;
                    }

                    if (options.Performance != null)
                    {
                        options.Performance.UseAsyncLogging = true;
                        options.Performance.EnableCaching = true;
                        options.Performance.MaxQueueSize = 10000;
                    }
                });
            }
            else
            {
                Log.Information("NetTracker is disabled or no connection string configured; skipping NetTracker registration.");
            }



            builder.Services.AddHttpContextAccessor();
            builder.Services.AddScoped<ICurrentLanguageService, LMS.API.Services.CurrentLanguageService>();

            var corsConfig = builder.Configuration.GetSection("Security:Cors").Get<CorsOptions>();
            var policyName = !string.IsNullOrWhiteSpace(corsConfig?.PolicyName) ? corsConfig.PolicyName : "DefaultPolicy";
            builder.Services.AddCors(options =>
            {
                options.AddDefaultPolicy(policy =>
                {
                    policy.SetIsOriginAllowed(_ => true)
                          .AllowAnyMethod()
                          .AllowAnyHeader()
                          .AllowCredentials()
                          .SetPreflightMaxAge(TimeSpan.FromHours(24));
                });
                options.AddPolicy(policyName, policy =>
                {
                    policy.SetIsOriginAllowed(_ => true)
                          .AllowAnyMethod()
                          .AllowAnyHeader()
                          .AllowCredentials()
                          .SetPreflightMaxAge(TimeSpan.FromHours(24));
                });
            });

            builder.Services
                .AddControllersWithViews(options => options.Conventions.Add(new HideNetTrackerControllersConvention()))
                .AddViewLocalization()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.Converters.Add(new IsoUtcDateTimeConverter());
                    options.JsonSerializerOptions.Converters.Add(new IsoUtcNullableDateTimeConverter());
                    options.JsonSerializerOptions.Encoder = System.Text.Encodings.Web.JavaScriptEncoder.Create(System.Text.Unicode.UnicodeRanges.All);
                })
                .ConfigureApiBehaviorOptions(options =>
                {
                    options.InvalidModelStateResponseFactory = context =>
                    {
                        var localizationProvider = context.HttpContext.RequestServices.GetService<ILocalizationProvider>();
                        var culture = context.HttpContext.Request.Headers["Accept-Language"].FirstOrDefault() ?? "en";
                        if (culture.Contains('-')) culture = culture.Split('-')[0];
                        if (culture.Length > 2) culture = culture.Substring(0, 2);
                        if (culture != "en" && culture != "ar") culture = "en";

                        var message = localizationProvider?.GetLocalizedString(LocalizationKeys.ExceptionMessages.InvalidModelState, culture);

                        var errors = context.ModelState
                            .Where(x => x.Value?.Errors.Count > 0)
                            .ToDictionary(
                                kvp => string.IsNullOrWhiteSpace(kvp.Key) ? "General" : kvp.Key,
                                kvp => kvp.Value!.Errors.Select(e =>
                                    !string.IsNullOrWhiteSpace(e.ErrorMessage)
                                        ? (localizationProvider?.GetLocalizedString(e.ErrorMessage, culture) ?? e.ErrorMessage)
                                        : (localizationProvider?.GetLocalizedString(LocalizationKeys.ExceptionMessages.InvalidModelState, culture) ?? "Invalid value.")
                                ).ToArray()
                            );

                        var response = ApiResponse<object?>.Error(errors, message, StatusCodes.Status400BadRequest);
                        return new BadRequestObjectResult(response);
                    };
                });

            builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(options =>
            {
                options.MultipartBodyLengthLimit = 4294967295L;
                options.ValueLengthLimit = int.MaxValue;
                options.MultipartHeadersLengthLimit = int.MaxValue;
            });

            builder.WebHost.ConfigureKestrel(options =>
            {
                options.Limits.MaxRequestBodySize = 4294967295L;
                options.Limits.KeepAliveTimeout = TimeSpan.FromHours(1);
                options.Limits.RequestHeadersTimeout = TimeSpan.FromMinutes(30);
                options.Limits.MinRequestBodyDataRate = null;
                options.Limits.MinResponseDataRate = null;
            });

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddMemoryCache();
            builder.Services.AddInMemoryRateLimiting();
            builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"));
            builder.Services.Configure<ClientRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"));
            builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

            builder.Services.AddApiVersioning(options =>
            {
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ReportApiVersions = true;
            }).AddMvc(options =>
            {
                options.Conventions.Controller(typeof(TrackerController)).IsApiVersionNeutral();
                options.Conventions.Controller(typeof(HealthController)).IsApiVersionNeutral();
                options.Conventions.Controller(typeof(HomeController)).IsApiVersionNeutral();
                options.Conventions.Controller(typeof(HttpTransactionsController)).IsApiVersionNeutral();
                options.Conventions.Controller(typeof(StatisticsController)).IsApiVersionNeutral();
            }).AddApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'VVV";
                options.SubstituteApiVersionInUrl = true;
            });

            builder.Services.AddLocalization();
            builder.Services.AddSingleton<IStringLocalizerFactory, JsonStringLocalizerFactory>();
            builder.Services.Configure<RequestLocalizationOptions>(options =>
            {
                var supportedCultures = new[] { new CultureInfo("En") };
                options.DefaultRequestCulture = new RequestCulture("En");
                options.SupportedCultures = supportedCultures;
                options.SupportedUICultures = supportedCultures;

                options.RequestCultureProviders = new List<IRequestCultureProvider>
                    {
                        new AcceptLanguageHeaderRequestCultureProvider(),
                        new QueryStringRequestCultureProvider(),
                        new CookieRequestCultureProvider()
                    };
            });

            builder.Services.AddSwaggerGen(options =>
            {
                options.AddAcceptLanguageHeader().IncludeApiXmlComments().AddJwtBearerSecurity().AddApiResponseExamples();
                options.DocumentFilter<RemoveApiVersionParameterDocumentFilter>();
            });

            builder.Services.AddOptions<SwaggerGenOptions>().Configure<IApiVersionDescriptionProvider>((options, provider) =>
            {
                options.AddVersionedSwaggerDocs(provider);
            });

            builder.Services.AddHttpContextAccessor();
            builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
            builder.Services.AddScoped<ICurrentLanguageService, CurrentLanguageService>();

            builder.Services.AddHsts(options =>
            {
                options.Preload = true;
                options.IncludeSubDomains = true;
                options.MaxAge = TimeSpan.FromDays(365);
            });


            var app = builder.Build();

            var forwardedHeadersOptions = new ForwardedHeadersOptions
            {
                ForwardedHeaders = Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.All
            };
            forwardedHeadersOptions.KnownNetworks.Clear();
            forwardedHeadersOptions.KnownProxies.Clear();
            app.UseForwardedHeaders(forwardedHeadersOptions);

            app.UseSecurityHeaders();

            if (app.Environment.EnvironmentName == "Production")
            {
                app.UseHsts();
            }

            app.UseStaticFiles();

            app.UseStaticFiles(new StaticFileOptions()
            {
                FileProvider = new CustomFileProvider(app.Environment.WebRootPath),
                RequestPath = "/files"
            });

            if (hasNetTracker)
            {
                app.UseWhen(context => !context.Request.Path.StartsWithSegments("/swagger")
                                    && !context.Request.Path.StartsWithSegments("/swagger-custom")
                                    && !context.Request.Path.StartsWithSegments("/files")
                                    && !context.Request.Path.StartsWithSegments("/net-tracker"),
                    subApp => subApp.UseNetTracker(app.Configuration));
            }

            app.UseCustomExceptionHandler();

            app.UseRequestLocalization();

            app.UseRouting();

            var corsSettings = app.Configuration.GetSection("Security:Cors").Get<CorsOptions>();
            var activePolicy = !string.IsNullOrWhiteSpace(corsSettings?.PolicyName) ? corsSettings.PolicyName : "DefaultPolicy";
            app.UseCors(activePolicy);

            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.InjectStylesheet("/swagger-custom/response-codes.css");
                c.InjectJavascript("/swagger-custom/response-codes.js");

                var apiVersionDescriptionProvider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();
                foreach (var description in apiVersionDescriptionProvider.ApiVersionDescriptions)
                {
                    c.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json", description.GroupName);
                }
            });

            app.MapGet("/", () => Results.Redirect("/swagger/"));

            app.UseIpRateLimiting();

            app.MapControllers();

            if (hasNetTracker)
            {
                app.MapControllerRoute(
                    name: "netTrackerDashboard",
                    pattern: "net-tracker/dashboard",
                    defaults: new { controller = "Tracker", action = "Index" });

                app.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Tracker}/{action=Index}/{id?}");
            }
            else
            {
                app.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            }

            if (hasNetTracker)
            {
                using (var scope = app.Services.CreateScope())
                {
                    var trackerDb = scope.ServiceProvider.GetService<NET_Tracker.Data.ApplicationDbContext>();
                    if (trackerDb != null)
                    {
                        var dbCreator = trackerDb.Database.GetService<IRelationalDatabaseCreator>();
                        try
                        {
                            if (dbCreator is RelationalDatabaseCreator relationalCreator)
                            {
                                if (!await relationalCreator.HasTablesAsync())
                                {
                                    await dbCreator.CreateTablesAsync();
                                    Log.Information("NetTracker HttpTransactions table created successfully.");
                                }
                            }

                            try
                            {
                                await trackerDb.Database.ExecuteSqlRawAsync(@"
                        IF EXISTS (SELECT 1 FROM sys.tables WHERE name = 'HttpTransactions')
                        AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_HttpTransactions_Perf' AND object_id = OBJECT_ID('HttpTransactions'))
                        BEGIN
                            CREATE NONCLUSTERED INDEX IX_HttpTransactions_Perf 
                            ON [HttpTransactions] ([Timestamp] DESC) 
                            INCLUDE ([StatusCode], [Method], [DurationMs], [Url]);
                        END");
                            }
                            catch { }
                        }
                        catch (Exception ex)
                        {
                            Log.Warning(ex, "Failed to verify or create NetTracker tables.");
                        }
                    }
                }
            }

            Log.Information("=== LMS API Started === Environment: {Environment}, BaseDir: {BaseDir}, ContentRoot: {ContentRoot}",
                app.Environment.EnvironmentName, AppContext.BaseDirectory, app.Environment.ContentRootPath);

            await app.RunAsync();
        }
    }

    public class HideNetTrackerControllersConvention : Microsoft.AspNetCore.Mvc.ApplicationModels.IControllerModelConvention
    {
        public void Apply(Microsoft.AspNetCore.Mvc.ApplicationModels.ControllerModel controller)
        {
            if ((controller.ControllerType.Assembly.FullName != null && controller.ControllerType.Assembly.FullName.Contains("NetTracker", StringComparison.OrdinalIgnoreCase)) ||
                (controller.ControllerType.Namespace != null && controller.ControllerType.Namespace.Contains("Tracker", StringComparison.OrdinalIgnoreCase)))
            {
                controller.ApiExplorer.IsVisible = false;
                foreach (var action in controller.Actions)
                {
                    action.ApiExplorer.IsVisible = false;
                }
            }
        }
    }
}
