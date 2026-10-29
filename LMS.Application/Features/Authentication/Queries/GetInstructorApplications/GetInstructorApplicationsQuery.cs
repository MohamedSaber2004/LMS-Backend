using LMS.Application.Common.Models;
using LMS.Application.Features.Authentication.DTOs;
using LMS.Domain.Enums;
using MediatR;

namespace LMS.Application.Features.Authentication.Queries.GetInstructorApplications
{
    public class GetInstructorApplicationsQuery
        : IRequest<Result<PagginatedResult<InstructorApplicationDto>>>
    {
        public InstructorApplicationStatus? Status { get; set; } = InstructorApplicationStatus.Pending;
        public int PageNumber { get; set; } = PagginatedResult<InstructorApplicationDto>.DefaultPageNumber;
        public int PageSize { get; set; } = PagginatedResult<InstructorApplicationDto>.DefaultPageSize;
    }
}
