using LMS.Application.Common.Models;
using LMS.Application.Features.Sessions.DTOs;
using LMS.Domain.Enums;
using MediatR;

namespace LMS.Application.Features.Sessions.Queries.GetTeacherRescheduleRequests
{
    public class GetTeacherRescheduleRequestsQuery : IRequest<Result<PagginatedResult<TeacherRescheduleRequestDto>>>
    {
        public RescheduleRequestStatus? Status { get; set; } = RescheduleRequestStatus.Pending;
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}

