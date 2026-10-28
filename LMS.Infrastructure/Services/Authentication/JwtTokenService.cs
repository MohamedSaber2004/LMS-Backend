using LMS.Application.Common.Interfaces;
using LMS.Application.Common.Options;
using LMS.Domain.Entities;
using LMS.Domain.Enums;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace LMS.Infrastructure.Services.Authentication
{
    public class JwtTokenService : IJwtTokenService
    {
        private readonly JwtSettings _settings;

        public JwtTokenService(IOptions<JwtSettings> settings)
        {
            _settings = settings.Value;
        }

        public string GenerateAccessToken(ApplicationUser user, IList<string> roles)
        {
            var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email!),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim("FullName", user.FullName)
        };

            var userTypesMask = roles
                .Select(r => Enum.TryParse<UserType>(r, ignoreCase: true, out var ut) ? (int)ut : 0)
                .Aggregate(0, (acc, val) => acc | val);

            claims.Add(new Claim("UserTypes", userTypesMask.ToString()));

            claims.Add(new Claim("TokenVersion", user.TokenVersion.ToString()));

            foreach (var role in roles)
            {
                claims.Add(new Claim("role", role));
            }


            foreach (var audience in GetAudiences())
            {
                claims.Add(new Claim(JwtRegisteredClaimNames.Aud, audience));
            }

            var secret = _settings.Secret;
            var issuer = _settings.Issuer;
            var expiryDays = _settings.ExpiryInDays;

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expiry = DateTime.UtcNow.AddDays(expiryDays);

            var token = new JwtSecurityToken(
                issuer,
                null,
                claims,
                expires: expiry,
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private string[] GetAudiences()
        {
            return !string.IsNullOrWhiteSpace(_settings.Audience)
                ? _settings.Audience.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                : Array.Empty<string>();
        }

        public string GenerateRefreshToken(ApplicationUser user)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("TokenType", "RefreshToken")
            };

            foreach (var audience in GetAudiences())
            {
                claims.Add(new Claim(JwtRegisteredClaimNames.Aud, audience));
            }

            var secret = _settings.Secret;
            var issuer = _settings.Issuer;
            var refreshDays = _settings.RefreshTokenExpiryDays > 0 ? _settings.RefreshTokenExpiryDays : 30;

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expiry = DateTime.UtcNow.AddDays(refreshDays);

            var token = new JwtSecurityToken(
                issuer,
                null,
                claims,
                expires: expiry,
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public Guid? ValidateRefreshToken(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                return null;

            try
            {
                var handler = new JwtSecurityTokenHandler();
                var principal = handler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Secret)),
                    ValidateIssuer = !string.IsNullOrWhiteSpace(_settings.Issuer),
                    ValidIssuer = _settings.Issuer,
                    ValidateAudience = GetAudiences().Length > 0,
                    ValidAudiences = GetAudiences(),
                    ValidateLifetime = true,
                    RequireExpirationTime = true,
                    ClockSkew = TimeSpan.Zero
                }, out _);

                if (principal.FindFirst("TokenType")?.Value != "RefreshToken")
                    return null;

                var subject = principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                    ?? principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                return Guid.TryParse(subject, out var userId) ? userId : null;
            }
            catch
            {
                return null;
            }
        }
    }
}
