using System;
using LMS.Application.Common.Models;
using LMS.Application.Features.LiveClasses.DTOs;
using MediatR;

namespace LMS.Application.Features.LiveClasses.Commands.RefreshToken
{
    public class RefreshTokenCommand : IRequest<Result<RefreshTokenDto>>
    {
        public Guid SessionId { get; set; }

        public RefreshTokenCommand(Guid sessionId)
        {
            SessionId = sessionId;
        }
    }
}

