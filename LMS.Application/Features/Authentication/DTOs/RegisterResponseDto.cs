using LMS.Domain.Enums;

namespace LMS.Application.Features.Authentication.DTOs
{
    public class RegisterResponseDto
    {
        public RegisterResponseDto() { }

        public RegisterResponseDto(
            Guid userId,
            string email,
            string fullName,
            UserType role,
            bool requiresApproval,
            Guid? instructorApplicationId = null)
        {
            UserId = userId;
            Email = email;
            FullName = fullName;
            Role = role;
            RequiresApproval = requiresApproval;
            InstructorApplicationId = instructorApplicationId;
        }

        public Guid UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public UserType Role { get; set; }
        public bool RequiresApproval { get; set; }
        public Guid? InstructorApplicationId { get; set; }
    }
}
