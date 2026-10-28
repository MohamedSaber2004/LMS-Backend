using LMS.Application.Common.Models;
using MediatR;

namespace LMS.Application.Features.Authentication.Commands.ForgetPassword
{
    public class ForgetPasswordCommand : IRequest<Result<string>>
    {
        public ForgetPasswordCommand() { }

        public ForgetPasswordCommand(string email)
        {
            Email = email;
        }

        public string Email { get; set; } = string.Empty;
    }
}
