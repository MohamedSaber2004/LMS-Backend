using LMS.Application.Common.Models;
using MediatR;

namespace LMS.Application.Features.Users.Commands.ChangeUserPassword
{
    public class ChangeUserPasswordCommand : IRequest<Result<string>>
    {
        public Guid UserId { get; set; }
        public string NewPassword { get; set; } = string.Empty;
        public string ConfirmNewPassword { get; set; } = string.Empty;
    }
}
