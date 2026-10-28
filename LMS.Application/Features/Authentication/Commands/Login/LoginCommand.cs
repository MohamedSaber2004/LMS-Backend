using LMS.Application.Common.Models;
using LMS.Application.Features.Authentication.DTOs;
using MediatR;

namespace LMS.Application.Features.Authentication.Commands.Login
{
    public class LoginCommand : IRequest<Result<AuthResponseDto>>
    {
        public LoginCommand() { }

        public LoginCommand(string email, string password)
        {
            Email = email;
            Password = password;
        }

        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
