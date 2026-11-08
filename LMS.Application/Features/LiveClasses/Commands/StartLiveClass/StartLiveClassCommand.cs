using System;
using LMS.Application.Common.Models;
using LMS.Application.Features.LiveClasses.DTOs;
using MediatR;

namespace LMS.Application.Features.LiveClasses.Commands.StartLiveClass
{
    public class StartLiveClassCommand : IRequest<Result<LiveClassTokenDto>>
    {
        public Guid SessionId { get; set; }

        public StartLiveClassCommand(Guid sessionId)
        {
            SessionId = sessionId;
        }
    }
}

