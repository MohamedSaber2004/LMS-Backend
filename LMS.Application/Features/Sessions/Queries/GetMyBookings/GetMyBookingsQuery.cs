using LMS.Application.Common.Models;
using LMS.Application.Features.Sessions.DTOs;
using LMS.Domain.Enums;
using MediatR;

namespace LMS.Application.Features.Sessions.Queries.GetMyBookings
{
    public class GetMyBookingsQuery
        : IRequest<Result<PagginatedResult<SessionDto>>>
    {
        public GetMyBookingsQuery() { }

        public SessionTab Tab { get; set; } = SessionTab.Upcoming;

        public int PageNumber { get; set; } = PagginatedResult<SessionDto>.DefaultPageNumber;

        public int PageSize { get; set; } = PagginatedResult<SessionDto>.DefaultPageSize;
    }
}
