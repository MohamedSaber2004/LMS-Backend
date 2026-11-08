using LMS.Application.Common.Models;
using LMS.Application.Features.Availability.DTOs;
using MediatR;

namespace LMS.Application.Features.Availability.Queries.CheckVacationConflicts
{
    public class CheckVacationConflictsQuery : IRequest<Result<VacationConflictResultDto>>
    {
        public DateTime? FromUtc { get; set; }
        public DateTime? ToUtc { get; set; }
    }
}
