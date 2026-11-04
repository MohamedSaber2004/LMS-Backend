using LMS.Application.Common.Models;
using LMS.Application.Features.Authentication.DTOs;
using MediatR;

namespace LMS.Application.Features.Authentication.Queries.GetMyInstructorApplication
{
    public class GetMyInstructorApplicationQuery : IRequest<Result<MyInstructorApplicationDto>>
    {
        public GetMyInstructorApplicationQuery() { }
    }
}
