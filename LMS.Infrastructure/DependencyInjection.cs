using LMS.Application.Common.Helpers;
using LMS.Application.Common.Interfaces;
using LMS.Application.Common.Models;
using LMS.Application.Common.Options;
using LMS.Domain.Entities;
using LMS.Domain.Repositories.Interfaces.Base;
using LMS.Infrastructure.Repositories.Implementations.Base;
using LMS.Infrastructure.Services.Attachments;
using LMS.Infrastructure.Services.Authentication;
using LMS.Infrastructure.Services.Email;
using LMS.Infrastructure.Services.Security;
using LMS.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace LMS.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<ISecurityHeadersService, SecurityHeadersService>();
            services.AddSingleton<ICorsPolicyService, CorsPolicyService>();
            services.AddSingleton<IAntiforgeryService, AntiforgeryService>();
            services.AddSingleton<IHstsService, HstsService>();

            services.AddHttpClient();

            services.AddScoped<IBaseFileService, BaseFileService>();
            services.AddScoped<IAudioValidator, AudioValidator>();
            services.AddScoped<IFileValidator, FileValidator>();
            services.AddScoped<IImageValidator, ImageValidator>();
            services.AddScoped<IVideoValidator, VideoValidator>();

            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped(typeof(IGenericRepository<,>), typeof(GenericRepository<,>));

            services.AddScoped<IJwtTokenService, JwtTokenService>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddSingleton<IOtpRateLimiter, OtpRateLimiter>();

            var identitySettings = configuration.GetSection("IdentityOptions").Get<LMS.Application.Common.Options.IdentityOptions>()
                ?? new LMS.Application.Common.Options.IdentityOptions();

            services.Configure<Microsoft.AspNetCore.Identity.IdentityOptions>(options =>
            {
                options.Password.RequireNonAlphanumeric = identitySettings.RequireNonAlphanumeric;
                options.Password.RequiredLength = identitySettings.RequiredLength > 0 ? identitySettings.RequiredLength : 6;
                options.Password.RequireDigit = identitySettings.RequiredDigit;
                options.Password.RequireLowercase = identitySettings.RequireLowercase;
                options.Password.RequireUppercase = identitySettings.RequireUppercase;
                options.Lockout.MaxFailedAccessAttempts = identitySettings.MaxFailedAttempts > 0 ? identitySettings.MaxFailedAttempts : 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
                options.SignIn.RequireConfirmedEmail = identitySettings.RequireConfirmedEmail;
                options.User.RequireUniqueEmail = identitySettings.RequireUniqueEmail;
                if (!string.IsNullOrWhiteSpace(identitySettings.AllowedUserNameCharacters))
                {
                    options.User.AllowedUserNameCharacters = identitySettings.AllowedUserNameCharacters;
                }
            });

            services.AddIdentityCore<ApplicationUser>()
                .AddRoles<IdentityRole<Guid>>()
                .AddEntityFrameworkStores<LmsDbContext>()
                .AddDefaultTokenProviders();

            var jwtSettings = configuration.GetSection(nameof(JwtSettings)).Get<JwtSettings>() ?? new JwtSettings();
            var secret = jwtSettings.Secret ?? string.Empty;

            var audiences = !string.IsNullOrWhiteSpace(jwtSettings.Audience)
                ? jwtSettings.Audience.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                : Array.Empty<string>();

            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
                ValidateIssuer = !string.IsNullOrWhiteSpace(jwtSettings.Issuer),
                ValidIssuer = jwtSettings.Issuer,
                ValidateAudience = audiences.Length > 0,
                ValidAudiences = audiences,
                RequireExpirationTime = true,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero,
            };

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.SaveToken = true;
                options.TokenValidationParameters = tokenValidationParameters;
                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        context.HttpContext.Items[AuthFailureReasons.ItemsKey] = context.Exception is SecurityTokenExpiredException
                            ? AuthFailureReasons.Expired
                            : AuthFailureReasons.Invalid;

                        return Task.CompletedTask;
                    },
                    OnTokenValidated = async context =>
                    {
                        var userManager = context.HttpContext.RequestServices.GetRequiredService<UserManager<ApplicationUser>>();
                        var userId = context.Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                            ?? context.Principal?.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
                        var versionClaim = context.Principal?.FindFirst("TokenVersion")?.Value;

                        if (!Guid.TryParse(userId, out _)
                            || !long.TryParse(versionClaim, out var tokenVersion))
                        {
                            context.HttpContext.Items[AuthFailureReasons.ItemsKey] = AuthFailureReasons.MissingClaims;
                            context.Fail(Localize(context.HttpContext, AuthFailureReasons.MissingClaims));
                            return;
                        }

                        var user = await userManager.FindByIdAsync(userId!);
                        if (user is null
                            || user.IsDeleted
                            || !user.IsActive
                            || user.TokenVersion != tokenVersion)
                        {
                            context.HttpContext.Items[AuthFailureReasons.ItemsKey] = AuthFailureReasons.Revoked;
                            context.Fail(Localize(context.HttpContext, AuthFailureReasons.Revoked));
                        }
                    },
                    OnChallenge = async context =>
                    {
                        context.HandleResponse();
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        context.Response.ContentType = "application/json; charset=utf-8";

                        var message = ResolveChallengeMessage(context.HttpContext);
                        var payload = ApiResponse.Error(new Dictionary<string, string[]>(), message, StatusCodes.Status401Unauthorized);

                        var jsonOptions = new System.Text.Json.JsonSerializerOptions
                        {
                            PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase,
                            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.Create(System.Text.Unicode.UnicodeRanges.All)
                        };

                        await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(payload, jsonOptions), Encoding.UTF8);
                    }
                };
            });

            services.AddAuthorization();

            return services;
        }

        private static string ResolveChallengeMessage(HttpContext httpContext)
            => Localize(httpContext, httpContext.Items[AuthFailureReasons.ItemsKey] as string);

        private static string Localize(HttpContext httpContext, string? failureReason)
        {
            var localizationKey = AuthFailureReasons.GetLocalizationKey(failureReason);
            var localization = httpContext.RequestServices.GetService<ILocalizationProvider>();

            return localization?.GetLocalizedString(localizationKey, httpContext.Request.Headers.AcceptLanguage.ToString()) ?? localizationKey;
        }
    }
}
