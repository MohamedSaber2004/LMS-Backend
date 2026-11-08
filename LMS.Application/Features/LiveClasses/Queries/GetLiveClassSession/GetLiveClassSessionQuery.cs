using System;
using LMS.Application.Common.Models;
using LMS.Application.Features.LiveClasses.DTOs;
using MediatR;

namespace LMS.Application.Features.LiveClasses.Queries.GetLiveClassSession
{
    public class GetLiveClassSessionQuery : IRequest<Result<LiveClassDetailsDto>>
    {
        public Guid SessionId { get; set; }

        public GetLiveClassSessionQuery(Guid sessionId)
        {
            SessionId = sessionId;
        }
    }
}

