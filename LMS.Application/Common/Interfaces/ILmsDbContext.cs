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
        DbSet<Learner> Learners { get; }
        DbSet<Instructor> Instructors { get; }
        DbSet<InstructorApplication> InstructorApplications { get; }
        DbSet<InstructorCertificate> InstructorCertificates { get; }
        DbSet<InstructorAvailabilitySlot> InstructorAvailabilitySlots { get; }
        DbSet<OnboardingQuestion> OnboardingQuestions { get; }
        DbSet<OnboardingOption> OnboardingOptions { get; }
        DbSet<OnboardingAnswer> OnboardingAnswers { get; }
        DbSet<HelpCategory> HelpCategories { get; }
        DbSet<HelpArticle> HelpArticles { get; }
        DbSet<IdentityRole<Guid>> Roles { get; }
        DbSet<IdentityUserRole<Guid>> UserRoles { get; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
