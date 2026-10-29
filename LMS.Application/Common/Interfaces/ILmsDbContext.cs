using LMS.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace LMS.Application.Common.Interfaces
{
    public interface ILmsDbContext : IAsyncDisposable
    {
        DbSet<Country> Countries { get; }
        DbSet<ApplicationUser> ApplicationUsers { get; }
        DbSet<UserRefreshToken> UserRefreshTokens { get; }
        DbSet<InstructorApplication> InstructorApplications { get; }
        DbSet<InstructorCertificate> InstructorCertificates { get; }
        DbSet<IdentityRole<Guid>> Roles { get; }
        DbSet<IdentityUserRole<Guid>> UserRoles { get; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
