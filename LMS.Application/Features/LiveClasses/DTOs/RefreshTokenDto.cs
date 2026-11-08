using System;

namespace LMS.Application.Features.LiveClasses.DTOs
{
    public class RefreshTokenDto
    {
        public string Token { get; set; } = string.Empty;
        public DateTime ExpiresAtUtc { get; set; }
    }
}

