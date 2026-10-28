using LMS.Application.Common.Models;
using MediatR;

namespace LMS.Application.Features.Authentication.Commands.VerifyOtp
{
    public class VerifyOtpCommand : IRequest<Result<string>>
    {
        public VerifyOtpCommand() { }

        public VerifyOtpCommand(string email, string otp)
        {
            Email = email;
            Otp = otp;
        }

        public string Email { get; set; } = string.Empty;
        public string Otp { get; set; } = string.Empty;
    }
}
