namespace LMS.Application.Features.Authentication.DTOs
{
    public class AuthResponseDto
    {
        public AuthResponseDto() { }

        public AuthResponseDto(
            Guid userId,
            string email,
            string fullName,
            IList<string> roles,
            string accessToken,
            string refreshToken,
            DateTime refreshTokenExpiration)
        {
            UserId = userId;
            Email = email;
            FullName = fullName;
            Roles = roles;
            AccessToken = accessToken;
            RefreshToken = refreshToken;
            RefreshTokenExpiration = refreshTokenExpiration;
        }

        public Guid UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public IList<string> Roles { get; set; } = new List<string>();
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime RefreshTokenExpiration { get; set; }
    }
}
