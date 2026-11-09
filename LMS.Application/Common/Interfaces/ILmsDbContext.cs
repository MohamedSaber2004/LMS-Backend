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
        DbSet<AvailabilityInstance> AvailabilityInstances { get; }
        DbSet<Session> Sessions { get; }
        DbSet<SessionBooking> SessionBookings { get; }
        DbSet<SessionAttendance> SessionAttendances { get; }
        DbSet<RescheduleRequest> RescheduleRequests { get; }
        DbSet<OnboardingQuestion> OnboardingQuestions { get; }
        DbSet<OnboardingOption> OnboardingOptions { get; }
        DbSet<OnboardingAnswer> OnboardingAnswers { get; }
        DbSet<HelpCategory> HelpCategories { get; }
        DbSet<HelpArticle> HelpArticles { get; }
        DbSet<Course> Courses { get; }
        DbSet<Lesson> Lessons { get; }
        DbSet<QuizQuestion> QuizQuestions { get; }
        DbSet<QuizOption> QuizOptions { get; }
        DbSet<CourseEnrollment> CourseEnrollments { get; }
        DbSet<CourseInstructor> CourseInstructors { get; }
        DbSet<LessonProgress> LessonProgress { get; }
        DbSet<QuizAttempt> QuizAttempts { get; }
        DbSet<QuizAttemptAnswer> QuizAttemptAnswers { get; }
        DbSet<LiveRoom> LiveRooms { get; }
        DbSet<LiveRoomParticipant> LiveRoomParticipants { get; }
        DbSet<IdentityRole<Guid>> Roles { get; }
        DbSet<IdentityUserRole<Guid>> UserRoles { get; }

        DbSet<TEntity> Set<TEntity>() where TEntity : class;

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
