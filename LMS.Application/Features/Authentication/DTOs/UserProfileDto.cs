using LMS.Domain.Enums;

namespace LMS.Application.Features.Authentication.DTOs
{
    public class UserProfileDto
    {
        public UserProfileDto() { }

        public UserProfileDto(
            Guid userId,
            string email,
            string fullName,
            string? profilePictureUrl,
            Guid? countryId,
            string? countryName,
            Language language,
            IList<string> roles,
            DateTime createdAt)
        {
            UserId = userId;
            Email = email;
            FullName = fullName;
            ProfilePictureUrl = profilePictureUrl;
            CountryId = countryId;
            CountryName = countryName;
            Language = language;
            Roles = roles;
            CreatedAt = createdAt;
        }

        public Guid UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? ProfilePictureUrl { get; set; }
        public Guid? CountryId { get; set; }
        public string? CountryName { get; set; }
        public Language Language { get; set; }
        public IList<string> Roles { get; set; } = new List<string>();
        public DateTime CreatedAt { get; set; }
    }
}
