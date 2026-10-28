using LMS.Application.Common.Models;
using MediatR;

namespace LMS.Application.Features.Authentication.Commands.ChangetPassword
{
    public class ChangePasswordCommand : IRequest<Result<string>>
    {
        public ChangePasswordCommand() { }

        public ChangePasswordCommand(string currentPassword, string newPassword, string confirmNewPassword)
        {
            CurrentPassword = currentPassword;
            NewPassword = newPassword;
            ConfirmNewPassword = confirmNewPassword;
        }

        public string CurrentPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
        public string ConfirmNewPassword { get; set; } = string.Empty;
    }
}
