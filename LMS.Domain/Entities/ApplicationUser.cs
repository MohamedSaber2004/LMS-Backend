using LMS.Domain.Enums;
using Microsoft.AspNetCore.Identity;

namespace LMS.Domain.Entities
{
    public class ApplicationUser : IdentityUser<Guid>
    {
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public string? UpdatedBy { get; set; }
        public string? DeletedBy { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsActive { get; set; } = true;

        public string FullName { get; private set; } = null!;
        public string? ProfilePictureUrl { get; private set; }
        public string? PasswordResetToken { get; private set; }
        public DateTime? PasswordResetTokenExpiry { get; private set; }
        public Language Language { get; private set; } = Language.En;

        public string? VerificationCode { get; private set; }
        public DateTime? VerificationCodeExpiry { get; private set; }
        public string? GoogleUserId { get; private set; }
        public long TokenVersion { get; private set; }
        public Guid? CountryId { get; set; }
        public virtual Country? Country { get; set; }

        public virtual ICollection<UserRefreshToken> RefreshTokens { get; private set; } = new List<UserRefreshToken>();

        public void SetVerificationCode(string code, DateTime expiry)
        {
            VerificationCode = code;
            VerificationCodeExpiry = expiry;
            UpdatedAt = DateTime.UtcNow;
        }

        public void ClearVerificationCode()
        {
            VerificationCode = null;
            VerificationCodeExpiry = null;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
