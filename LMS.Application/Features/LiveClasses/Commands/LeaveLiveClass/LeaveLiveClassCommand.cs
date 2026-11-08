using System;
using LMS.Application.Common.Models;
using MediatR;

namespace LMS.Application.Features.LiveClasses.Commands.LeaveLiveClass
{
    public class LeaveLiveClassCommand : IRequest<Result<bool>>
    {
        public Guid SessionId { get; set; }

        public LeaveLiveClassCommand(Guid sessionId)
        {
            SessionId = sessionId;
        }
    }
}

