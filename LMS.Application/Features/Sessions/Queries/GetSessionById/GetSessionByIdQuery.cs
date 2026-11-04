using LMS.Application.Common.Models;
using LMS.Application.Features.Sessions.DTOs;
using MediatR;

namespace LMS.Application.Features.Sessions.Queries.GetSessionById
{
    public class GetSessionByIdQuery : IRequest<Result<SessionDetailDto>>
    {
        public GetSessionByIdQuery() { }

        public Guid Id { get; set; }
    }
}
