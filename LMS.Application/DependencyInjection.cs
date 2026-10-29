using FluentValidation;
using LMS.Application.Common.Behaviours;
using LMS.Application.Common.Interfaces;
using LMS.Application.Common.Options;
using LMS.Application.Common.Services;
using LMS.Application.Features.Attachment.Commands.DownloadAttachment;
using LMS.Application.Features.Attachment.Commands.UpdateAttachment;
using LMS.Application.Features.Attachment.Commands.UploadAttachment;
using LMS.Application.Features.Attachment.Commands.UploadMultipleAttachments;
using LMS.Application.Features.Authentication.Commands.ChangetPassword;
using LMS.Application.Features.Authentication.Commands.DeleteAccount;
using LMS.Application.Features.Authentication.Commands.ForgetPassword;
using LMS.Application.Features.Authentication.Commands.Login;
using LMS.Application.Features.Authentication.Commands.Logout;
using LMS.Application.Features.Authentication.Commands.RefreshToken;
using LMS.Application.Features.Authentication.Commands.Register;
using LMS.Application.Features.Authentication.Commands.RejectInstructorApplication;
using LMS.Application.Features.Authentication.Commands.ResendOtp;
using LMS.Application.Features.Authentication.Commands.ResetPassword;
using LMS.Application.Features.Authentication.Commands.UpdateUserInformation;
using LMS.Application.Features.Authentication.Commands.VerifyOtp;
using LMS.Application.Features.Authentication.Commands.ApproveInstructorApplication;
using LMS.Application.Features.Authentication.Queries.GetInstructorApplications;
using LMS.Application.Features.Authentication.Queries.GetUserProfile;
using LMS.Application.Features.Countries.Commands.AddCountry;
using LMS.Application.Features.Countries.Commands.DeleteCountry;
using LMS.Application.Features.Countries.Commands.UpdateCountry;
using LMS.Application.Features.Countries.Queries.GetAllCountries;
using LMS.Application.Features.Countries.Queries.GetAllCountriesPagginated;
using LMS.Application.Features.Countries.Queries.GetCountryById;
using LMS.Application.Features.Users.Commands.AddUser;
using LMS.Application.Features.Users.Commands.ChangeUserPassword;
using LMS.Application.Features.Users.Commands.DeleteUser;
using LMS.Application.Features.Users.Commands.UpdateUser;
using LMS.Application.Features.Users.Queries.GetUserById;
using LMS.Application.Features.Users.Queries.GetUsersPagginated;
using LMS.Application.Localization;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LMS.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services,IConfiguration configuration)
        {
            services.Configure<SecurityOptions>(configuration.GetSection(SecurityOptions.SectionName));
            services.Configure<SecurityHeadersOptions>(configuration.GetSection("Security:Headers"));
            services.Configure<CorsOptions>(configuration.GetSection("Security:Cors"));
            services.Configure<AntiforgeryOptions>(configuration.GetSection("Security:Antiforgery"));
            services.Configure<RequestLimitsOptions>(configuration.GetSection("Security:RequestLimits"));
            services.Configure<HstsOptions>(configuration.GetSection("Security:Hsts"));
            services.Configure<IpRateLimitingOptions>(configuration.GetSection(IpRateLimitingOptions.SectionName));
            services.Configure<JwtSettings>(configuration.GetSection(nameof(JwtSettings)));
            services.Configure<IdentityOptions>(configuration.GetSection(nameof(IdentityOptions)));
            services.Configure<EmailSettings>(configuration.GetSection(nameof(EmailSettings)));
            services.Configure<GoogleAuthSettings>(configuration.GetSection(nameof(GoogleAuthSettings)));
            services.Configure<UploadPathsOptions>(configuration.GetSection(UploadPathsOptions.SectionName));
            services.Configure<VideoCompressionOptions>(configuration.GetSection(VideoCompressionOptions.SectionName));

            UploadPaths.Configure(configuration);

            services.AddMediatR(typeof(DependencyInjection).Assembly);

            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(UnhandledExceptionBehaviour<,>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));

            services.AddSingleton<ILocalizationProvider, JsonLocalizationProvider>();

            services.AddTransient<IValidator<UploadAttachmentCommand>, UploadAttachmentCommandValidator>();
            services.AddTransient<IValidator<DownloadAttachmentCommand>, DownloadAttachmentCommandValidator>();
            services.AddTransient<IValidator<UploadMultipleAttachmentsCommand>, UploadMultipleAttachmentsCommandValidator>();
            services.AddTransient<IValidator<UpdateAttachmentCommand>, UpdateAttachmentCommandValidator>();
            services.AddTransient<IValidator<LoginCommand>, LoginCommandValidator>();
            services.AddTransient<IValidator<RegisterCommand>, RegisterCommandValidator>();
            services.AddTransient<IValidator<ApproveInstructorApplicationCommand>, ApproveInstructorApplicationCommandValidator>();
            services.AddTransient<IValidator<RejectInstructorApplicationCommand>, RejectInstructorApplicationCommandValidator>();
            services.AddTransient<IValidator<GetInstructorApplicationsQuery>, GetInstructorApplicationsQueryValidator>();
            services.AddTransient<IValidator<ForgetPasswordCommand>, ForgetPasswordCommandValidator>();
            services.AddTransient<IValidator<ResendOtpCommand>, ResendOtpCommandValidator>();
            services.AddTransient<IValidator<ChangePasswordCommand>, ChangePasswordCommandValidator>();
            services.AddTransient<IValidator<DeleteAccountCommand>, DeleteAccountCommandValidator>();
            services.AddTransient<IValidator<LogoutCommand>, LogoutCommandValidator>();
            services.AddTransient<IValidator<RefreshTokenCommand>, RefreshTokenCommandValidator>();
            services.AddTransient<IValidator<ResetPasswordCommand>, ResetPasswordCommandValidator>();
            services.AddTransient<IValidator<UpdateUserInformationCommand>, UpdateUserInformationCommandValidator>();
            services.AddTransient<IValidator<VerifyOtpCommand>, VerifyOtpCommandValidator>();
            services.AddTransient<IValidator<GetUserProfileQuery>, GetUserProfileQueryValidator>();
            services.AddTransient<IValidator<AddCountryCommand>, AddCountryCommandValidator>();
            services.AddTransient<IValidator<UpdateCountryCommand>, UpdateCountryCommandValidator>();
            services.AddTransient<IValidator<DeleteCountryCommand>, DeleteCountryCommandValidator>();
            services.AddTransient<IValidator<GetAllCountriesQuery>, GetAllCountriesQueryValidator>();
            services.AddTransient<IValidator<GetAllCountriesPagginatedQuery>, GetAllCountriesPagginatedQueryValidator>();
            services.AddTransient<IValidator<GetCountryByIdQuery>, GetCountryByIdQueryValidator>();
            services.AddTransient<IValidator<AddUserCommand>, AddUserCommandValidator>();
            services.AddTransient<IValidator<UpdateUserCommand>, UpdateUserCommandValidator>();
            services.AddTransient<IValidator<DeleteUserCommand>, DeleteUserCommandValidator>();
            services.AddTransient<IValidator<ChangeUserPasswordCommand>, ChangeUserPasswordCommandValidator>();
            services.AddTransient<IValidator<GetUserByIdQuery>, GetUserByIdQueryValidator>();
            services.AddTransient<IValidator<GetUsersPagginatedQuery>, GetUsersPagginatedQueryValidator>();

            return services;
        }
    }
}
