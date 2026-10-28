using LMS.Application.Common.Models;
using LMS.Application.Localization;
using MediatR;
using Microsoft.Extensions.Localization;

namespace LMS.Application.Features.Authentication.Commands.VerifyOtp
{
    public class VerifyOtpCommandHandler : IRequestHandler<VerifyOtpCommand, Result<string>>
    {
        private readonly IStringLocalizer<Messages> _localizer;

        public VerifyOtpCommandHandler(IStringLocalizer<Messages> localizer)
        {
            _localizer = localizer;
        }

        public Task<Result<string>> Handle(VerifyOtpCommand request, CancellationToken cancellationToken)
        {
            var message = _localizer[LocalizationKeys.Authentication.OtpVerifiedSuccess].Value;
            return Task.FromResult(Result<string>.Success(message, message));
        }
    }
}
