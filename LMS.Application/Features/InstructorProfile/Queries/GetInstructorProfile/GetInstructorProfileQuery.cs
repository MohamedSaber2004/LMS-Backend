using LMS.Application.Common.Models;
using LMS.Application.Features.InstructorProfile.DTOs;
using MediatR;

namespace LMS.Application.Features.InstructorProfile.Queries.GetInstructorProfile
{
    public class GetInstructorProfileQuery : IRequest<Result<InstructorProfileDto>>
    {
        public GetInstructorProfileQuery() { }
    }
}
