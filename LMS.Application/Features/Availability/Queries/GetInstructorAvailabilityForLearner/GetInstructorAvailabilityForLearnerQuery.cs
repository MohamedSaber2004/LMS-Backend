using LMS.Application.Common.Models;
using LMS.Application.Features.Availability.DTOs;
using MediatR;

namespace LMS.Application.Features.Availability.Queries.GetInstructorAvailabilityForLearner
{
    public class GetInstructorAvailabilityForLearnerQuery : IRequest<Result<InstructorAvailabilityDto>>
    {
        public GetInstructorAvailabilityForLearnerQuery() { }

        public Guid InstructorId { get; set; }
    }
}
