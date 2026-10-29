using LMS.Application.Common.Models;
using LMS.Application.Features.Users.DTOs;
using LMS.Domain.Enums;
using MediatR;

namespace LMS.Application.Features.Users.Queries.GetUsersPagginated
{
    public class GetUsersPagginatedQuery : IRequest<Result<PagginatedResult<UserDto>>>
    {
        public string? Search { get; set; }
        public UserType? Role { get; set; }
        public bool? IsActive { get; set; }
        public int PageNumber { get; set; } = PagginatedResult<UserDto>.DefaultPageNumber;
        public int PageSize { get; set; } = PagginatedResult<UserDto>.DefaultPageSize;
    }
}
