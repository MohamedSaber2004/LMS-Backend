using LMS.Application.Common.Models;
using LMS.Application.Features.Sessions.DTOs;
using MediatR;

namespace LMS.Application.Features.Sessions.Commands.CancelSession
{
    /// <summary>
    /// One call for both sides. The actor decides the lock: the teacher gets 24 h before the start, the
    /// learner 12 h. A learner cancels their own seat; the session itself goes with it only when nobody
    /// is left holding one.
    /// </summary>
    public class CancelSessionCommand : IRequest<Result<SessionDto>>
    {
        public Guid Id { get; set; }

        public string? Reason { get; set; }
    }
}
