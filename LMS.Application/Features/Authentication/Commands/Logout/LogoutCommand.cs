using LMS.Application.Common.Models;
using MediatR;

namespace LMS.Application.Features.Authentication.Commands.Logout
{
    public class LogoutCommand : IRequest<Result<string>>
    {
        public LogoutCommand() { }

        public LogoutCommand(string? refreshToken)
        {
            RefreshToken = refreshToken;
        }

        public string? RefreshToken { get; set; }
    }
}
