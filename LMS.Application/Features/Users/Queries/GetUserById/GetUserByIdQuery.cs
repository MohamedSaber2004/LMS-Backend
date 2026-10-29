using LMS.Application.Common.Models;
using LMS.Application.Features.Users.DTOs;
using MediatR;

namespace LMS.Application.Features.Users.Queries.GetUserById
{
    public class GetUserByIdQuery : IRequest<Result<UserDto>>
    {
        public Guid Id { get; set; }
    }
}
