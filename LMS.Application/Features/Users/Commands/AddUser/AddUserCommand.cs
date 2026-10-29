using LMS.Application.Common.Models;
using LMS.Application.Features.Users.DTOs;
using LMS.Domain.Enums;
using MediatR;

namespace LMS.Application.Features.Users.Commands.AddUser
{
    public class AddUserCommand : IRequest<Result<UserDto>>
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public Guid CountryId { get; set; }
        public UserType Role { get; set; } = UserType.Learner;
        public bool IsActive { get; set; } = true;
    }
}
