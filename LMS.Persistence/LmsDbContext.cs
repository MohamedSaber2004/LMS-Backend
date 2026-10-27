using LMS.API.Services;
using LMS.Application.Common.Interfaces;
using LMS.Domain.Common;
using LMS.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace LMS.Persistence
{
    public class LmsDbContext: IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid,
        IdentityUserClaim<Guid>, IdentityUserRole<Guid>, IdentityUserLogin<Guid>,
        IdentityRoleClaim<Guid>, IdentityUserToken<Guid>>, ILmsDbContext
    {
        private readonly ICurrentUserService _currentUserService;

        public DbSet<Country> Countries { get; set; }
        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
        public DbSet<UserRefreshToken> UserRefreshTokens { get; set; }

        public LmsDbContext(
            ICurrentUserService currentUserService,
            DbContextOptions<LmsDbContext> options) : base(options)
        {
            _currentUserService = currentUserService;
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.ApplyConfigurationsFromAssembly(typeof(LmsDbContext).Assembly,
                type => type.Namespace is not null && type.Namespace.EndsWith("Configurations"));

            var dateTimeConverter = new Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<DateTime, DateTime>(
                v => v.Kind == DateTimeKind.Utc ? v : DateTime.SpecifyKind(v.ToUniversalTime(), DateTimeKind.Utc),
                v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

            var nullableDateTimeConverter = new Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<DateTime?, DateTime?>(
                v => v.HasValue ? (v.Value.Kind == DateTimeKind.Utc ? v : DateTime.SpecifyKind(v.Value.ToUniversalTime(), DateTimeKind.Utc)) : v,
                v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : v);

            foreach (var entityType in builder.Model.GetEntityTypes())
            {
                foreach (var property in entityType.GetProperties())
                {
                    if (property.ClrType == typeof(DateTime))
                    {
                        property.SetValueConverter(dateTimeConverter);
                    }
                    else if (property.ClrType == typeof(DateTime?))
                    {
                        property.SetValueConverter(nullableDateTimeConverter);
                    }
                }
            }
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var userId = _currentUserService?.UserId.ToString() ?? "System";

            foreach (var entry in ChangeTracker.Entries<BaseEntity>())
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.MarkAsCreated(userId);
                        break;
                    case EntityState.Modified:
                        entry.Entity.MarkAsUpdated(userId);
                        break;
                    case EntityState.Deleted:
                        entry.State = EntityState.Modified;
                        entry.Entity.MarkAsDeleted(userId);
                        break;
                }
            }

            foreach (var entry in ChangeTracker.Entries().Where(e => e.Entity is ApplicationUser && e.State != EntityState.Detached))
            {
                var user = (ApplicationUser)entry.Entity;
                switch (entry.State)
                {
                    case EntityState.Added:
                        user.CreatedAt = DateTime.UtcNow;
                        user.CreatedBy = userId;
                        user.IsActive = true;
                        break;
                    case EntityState.Modified:
                        user.UpdatedAt = DateTime.UtcNow;
                        user.UpdatedBy = userId;
                        break;
                }
            }

            return await base.SaveChangesAsync(cancellationToken);
        }
    }
}
