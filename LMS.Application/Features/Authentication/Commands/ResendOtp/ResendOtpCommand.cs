using LMS.Application.Common.Models;
using MediatR;

namespace LMS.Application.Features.Authentication.Commands.ResendOtp
{
    public class ResendOtpCommand : IRequest<Result<string>>
    {
        public ResendOtpCommand() { }

        public ResendOtpCommand(string email)
        {
            Email = email;
        }

        public string Email { get; set; } = string.Empty;
    }
}
