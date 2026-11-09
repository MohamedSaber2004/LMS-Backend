using LMS.Application.Common.Models;
using LMS.Application.Features.Authentication.DTOs;
using MediatR;

namespace LMS.Application.Features.Authentication.Queries.GetInstructorApplicationById
{
    public class GetInstructorApplicationByIdQuery : IRequest<Result<InstructorApplicationDto>>
    {
        public Guid Id { get; set; }

        public GetInstructorApplicationByIdQuery() { }

        public GetInstructorApplicationByIdQuery(Guid id)
        {
            Id = id;
        }
    }
}

