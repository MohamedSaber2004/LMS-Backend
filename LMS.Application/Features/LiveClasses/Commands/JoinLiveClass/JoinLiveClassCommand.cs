using System;
using LMS.Application.Common.Models;
using LMS.Application.Features.LiveClasses.DTOs;
using MediatR;

namespace LMS.Application.Features.LiveClasses.Commands.JoinLiveClass
{
    public class JoinLiveClassCommand : IRequest<Result<LiveClassTokenDto>>
    {
        public Guid SessionId { get; set; }

        public JoinLiveClassCommand(Guid sessionId)
        {
            SessionId = sessionId;
        }
    }
}

