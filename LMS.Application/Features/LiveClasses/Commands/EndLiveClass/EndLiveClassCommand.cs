using System;
using LMS.Application.Common.Models;
using MediatR;

namespace LMS.Application.Features.LiveClasses.Commands.EndLiveClass
{
    public class EndLiveClassCommand : IRequest<Result<bool>>
    {
        public Guid SessionId { get; set; }

        public EndLiveClassCommand(Guid sessionId)
        {
            SessionId = sessionId;
        }
    }
}

