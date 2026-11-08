using LMS.Application.Common.Models;
using LMS.Application.Features.Sessions.DTOs;
using MediatR;

namespace LMS.Application.Features.Sessions.Queries.GetSessionParticipants
{
    public class GetSessionParticipantsQuery : IRequest<Result<IReadOnlyCollection<SessionParticipantDto>>>
    {
        public Guid SessionId { get; set; }
    }
}
