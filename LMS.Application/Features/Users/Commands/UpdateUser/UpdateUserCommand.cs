using LMS.Application.Common.Models;
using LMS.Application.Features.Users.DTOs;
using LMS.Domain.Enums;
using MediatR;

namespace LMS.Application.Features.Users.Commands.UpdateUser
{
    public class UpdateUserCommand : IRequest<Result<UserDto>>
    {
        public Guid Id { get; set; }
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public Guid? CountryId { get; set; }
        public UserType? Role { get; set; }
        public bool? IsActive { get; set; }
    }
}
