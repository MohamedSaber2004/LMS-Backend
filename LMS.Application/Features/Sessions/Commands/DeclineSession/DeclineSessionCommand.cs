using LMS.Application.Common.Models;
using LMS.Application.Features.Sessions.DTOs;
using MediatR;

namespace LMS.Application.Features.Sessions.Commands.DeclineSession
{
    public class DeclineSessionCommand : IRequest<Result<SessionDto>>
    {
        public Guid Id { get; set; }
    }
}
