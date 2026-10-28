using LMS.Application.Common.Models;
using MediatR;

namespace LMS.Application.Features.Authentication.Commands.ResetPassword
{
    public class ResetPasswordCommand : IRequest<Result<string>>
    {
        public ResetPasswordCommand() { }

        public ResetPasswordCommand(string email, string otp, string newPassword, string confirmNewPassword)
        {
            Email = email;
            Otp = otp;
            NewPassword = newPassword;
            ConfirmNewPassword = confirmNewPassword;
        }

        public string Email { get; set; } = string.Empty;
        public string Otp { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
        public string ConfirmNewPassword { get; set; } = string.Empty;
    }
}
