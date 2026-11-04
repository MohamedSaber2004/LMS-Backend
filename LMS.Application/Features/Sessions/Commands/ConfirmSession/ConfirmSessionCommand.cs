using LMS.Application.Common.Models;
using LMS.Application.Features.Sessions.DTOs;
using MediatR;

namespace LMS.Application.Features.Sessions.Commands.ConfirmSession
{
    public class ConfirmSessionCommand : IRequest<Result<SessionDto>>
    {
        public Guid Id { get; set; }
    }
}
