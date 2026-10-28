using LMS.Application.Common.Models;
using LMS.Application.Features.Authentication.DTOs;
using MediatR;

namespace LMS.Application.Features.Authentication.Commands.RefreshToken
{
    public class RefreshTokenCommand : IRequest<Result<AuthResponseDto>>
    {
        public RefreshTokenCommand() { }

        public RefreshTokenCommand(string refreshToken)
        {
            RefreshToken = refreshToken;
        }

        public string RefreshToken { get; set; } = string.Empty;
    }
}
