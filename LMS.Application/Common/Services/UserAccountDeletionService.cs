using LMS.Application.Common.Interfaces;
using LMS.Domain.Entities;
using LMS.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LMS.Application.Common.Services
{
    public class UserAccountDeletionService : IUserAccountDeletionService
    {
        private const int CertificateFilePlace = (int)MediaKind.Document;
        private const int IntroVideoFilePlace = (int)MediaKind.Video;
        private const int ImageFilePlace = (int)MediaKind.Image;

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILmsDbContext _context;
        private readonly IFileValidator _fileValidator;
        private readonly IImageValidator _imageValidator;
        private readonly IVideoValidator _videoValidator;
        private readonly ILogger<UserAccountDeletionService> _logger;

        public UserAccountDeletionService(
            UserManager<ApplicationUser> userManager,
            ILmsDbContext context,
            IFileValidator fileValidator,
            IImageValidator imageValidator,
            IVideoValidator videoValidator,
            ILogger<UserAccountDeletionService> logger)
        {
            _userManager = userManager;
            _context = context;
            _fileValidator = fileValidator;
            _imageValidator = imageValidator;
            _videoValidator = videoValidator;
            _logger = logger;
        }

        public async Task HardDeleteUserAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user is null)
            {
                return;
            }

            var filesToDelete = new List<(string Path, int Place, bool IsVideo, bool IsImage)>();

            if (!string.IsNullOrWhiteSpace(user.ProfilePictureUrl))
            {
                filesToDelete.Add((user.ProfilePictureUrl, ImageFilePlace, false, true));
            }

            var instructor = await _context.Instructors.IgnoreQueryFilters()
                .FirstOrDefaultAsync(i => i.UserId == userId, cancellationToken);

            if (instructor is not null)
            {
                var certificates = await _context.InstructorCertificates.IgnoreQueryFilters()
                    .Where(c => c.InstructorId == instructor.Id)
                    .Select(c => new { c.FilePath, c.Type })
                    .ToListAsync(cancellationToken);

                foreach (var cert in certificates)
                {
                    if (string.IsNullOrWhiteSpace(cert.FilePath))
                    {
                        continue;
                    }

                    if (cert.Type == InstructorCertificateType.IntroVideo)
                    {
                        filesToDelete.Add((cert.FilePath, IntroVideoFilePlace, true, false));
                    }
                    else
                    {
                        filesToDelete.Add((cert.FilePath, CertificateFilePlace, false, false));
                    }
                }
            }

            await _context.LiveRoomParticipants.IgnoreQueryFilters()
                .Where(p => p.UserId == userId)
                .ExecuteDeleteAsync(cancellationToken);

            var hostedRoomIds = await _context.LiveRooms.IgnoreQueryFilters()
                .Where(r => r.HostUserId == userId)
                .Select(r => r.Id)
                .ToListAsync(cancellationToken);

            if (hostedRoomIds.Count > 0)
            {
                await _context.LiveRoomParticipants.IgnoreQueryFilters()
                    .Where(p => hostedRoomIds.Contains(p.RoomId))
                    .ExecuteDeleteAsync(cancellationToken);

                await _context.LiveRooms.IgnoreQueryFilters()
                    .Where(r => hostedRoomIds.Contains(r.Id))
                    .ExecuteDeleteAsync(cancellationToken);
            }

            await _context.OnboardingAnswers.IgnoreQueryFilters()
                .Where(a => a.UserId == userId)
                .ExecuteDeleteAsync(cancellationToken);

            await _context.UserRefreshTokens.IgnoreQueryFilters()
                .Where(t => t.UserId == userId)
                .ExecuteDeleteAsync(cancellationToken);

            var learner = await _context.Learners.IgnoreQueryFilters()
                .FirstOrDefaultAsync(l => l.UserId == userId, cancellationToken);

            if (learner is not null)
            {
                var learnerId = learner.Id;

                var quizAttemptIds = await _context.QuizAttempts.IgnoreQueryFilters()
                    .Where(q => q.LearnerId == learnerId)
                    .Select(q => q.Id)
                    .ToListAsync(cancellationToken);

                if (quizAttemptIds.Count > 0)
                {
                    await _context.QuizAttemptAnswers.IgnoreQueryFilters()
                        .Where(a => quizAttemptIds.Contains(a.QuizAttemptId))
                        .ExecuteDeleteAsync(cancellationToken);

                    await _context.QuizAttempts.IgnoreQueryFilters()
                        .Where(q => quizAttemptIds.Contains(q.Id))
                        .ExecuteDeleteAsync(cancellationToken);
                }

                await _context.LessonProgress.IgnoreQueryFilters()
                    .Where(lp => lp.LearnerId == learnerId)
                    .ExecuteDeleteAsync(cancellationToken);

                await _context.CourseEnrollments.IgnoreQueryFilters()
                    .Where(ce => ce.LearnerId == learnerId)
                    .ExecuteDeleteAsync(cancellationToken);

                await _context.SessionAttendances.IgnoreQueryFilters()
                    .Where(sa => sa.LearnerId == learnerId)
                    .ExecuteDeleteAsync(cancellationToken);

                await _context.SessionBookings.IgnoreQueryFilters()
                    .Where(sb => sb.LearnerId == learnerId)
                    .ExecuteDeleteAsync(cancellationToken);

                await _context.Learners.IgnoreQueryFilters()
                    .Where(l => l.Id == learnerId)
                    .ExecuteDeleteAsync(cancellationToken);
            }

            if (instructor is not null)
            {
                var instructorId = instructor.Id;

                await _context.CourseInstructors.IgnoreQueryFilters()
                    .Where(ci => ci.InstructorId == instructorId)
                    .ExecuteDeleteAsync(cancellationToken);

                var sessionIds = await _context.Sessions.IgnoreQueryFilters()
                    .Where(s => s.InstructorId == instructorId)
                    .Select(s => s.Id)
                    .ToListAsync(cancellationToken);

                if (sessionIds.Count > 0)
                {
                    var sessionLiveRoomIds = await _context.LiveRooms.IgnoreQueryFilters()
                        .Where(r => sessionIds.Contains(r.SessionId))
                        .Select(r => r.Id)
                        .ToListAsync(cancellationToken);

                    if (sessionLiveRoomIds.Count > 0)
                    {
                        await _context.LiveRoomParticipants.IgnoreQueryFilters()
                            .Where(p => sessionLiveRoomIds.Contains(p.RoomId))
                            .ExecuteDeleteAsync(cancellationToken);

                        await _context.LiveRooms.IgnoreQueryFilters()
                            .Where(r => sessionLiveRoomIds.Contains(r.Id))
                            .ExecuteDeleteAsync(cancellationToken);
                    }

                    await _context.RescheduleRequests.IgnoreQueryFilters()
                        .Where(rr => sessionIds.Contains(rr.SessionId))
                        .ExecuteDeleteAsync(cancellationToken);

                    await _context.SessionAttendances.IgnoreQueryFilters()
                        .Where(sa => sessionIds.Contains(sa.SessionId))
                        .ExecuteDeleteAsync(cancellationToken);

                    await _context.SessionBookings.IgnoreQueryFilters()
                        .Where(sb => sessionIds.Contains(sb.SessionId))
                        .ExecuteDeleteAsync(cancellationToken);

                    await _context.Sessions.IgnoreQueryFilters()
                        .Where(s => sessionIds.Contains(s.Id))
                        .ExecuteDeleteAsync(cancellationToken);
                }

                await _context.AvailabilityInstances.IgnoreQueryFilters()
                    .Where(ai => ai.InstructorId == instructorId)
                    .ExecuteDeleteAsync(cancellationToken);

                await _context.InstructorAvailabilitySlots.IgnoreQueryFilters()
                    .Where(ias => ias.InstructorId == instructorId)
                    .ExecuteDeleteAsync(cancellationToken);

                await _context.InstructorCertificates.IgnoreQueryFilters()
                    .Where(ic => ic.InstructorId == instructorId)
                    .ExecuteDeleteAsync(cancellationToken);

                await _context.InstructorApplications.IgnoreQueryFilters()
                    .Where(ia => ia.InstructorId == instructorId)
                    .ExecuteDeleteAsync(cancellationToken);

                await _context.Instructors.IgnoreQueryFilters()
                    .Where(i => i.Id == instructorId)
                    .ExecuteDeleteAsync(cancellationToken);
            }

            await _context.Sessions.IgnoreQueryFilters()
                .Where(s => s.CancelledByUserId == userId)
                .ExecuteUpdateAsync(s => s.SetProperty(x => x.CancelledByUserId, (Guid?)null), cancellationToken);

            await _context.SessionBookings.IgnoreQueryFilters()
                .Where(sb => sb.CancelledByUserId == userId)
                .ExecuteUpdateAsync(sb => sb.SetProperty(x => x.CancelledByUserId, (Guid?)null), cancellationToken);

            await _context.InstructorApplications.IgnoreQueryFilters()
                .Where(ia => ia.ReviewedByUserId == userId)
                .ExecuteUpdateAsync(ia => ia.SetProperty(x => x.ReviewedByUserId, (Guid?)null), cancellationToken);

            await _context.RescheduleRequests.IgnoreQueryFilters()
                .Where(rr => rr.RequestedByUserId == userId)
                .ExecuteDeleteAsync(cancellationToken);

            await _context.RescheduleRequests.IgnoreQueryFilters()
                .Where(rr => rr.RespondedByUserId == userId)
                .ExecuteUpdateAsync(rr => rr.SetProperty(x => x.RespondedByUserId, (Guid?)null), cancellationToken);

            // Delete ASP.NET Identity user mappings
            await _context.UserRoles.Where(ur => ur.UserId == userId).ExecuteDeleteAsync(cancellationToken);
            await _context.Set<IdentityUserClaim<Guid>>().Where(uc => uc.UserId == userId).ExecuteDeleteAsync(cancellationToken);
            await _context.Set<IdentityUserLogin<Guid>>().Where(ul => ul.UserId == userId).ExecuteDeleteAsync(cancellationToken);
            await _context.Set<IdentityUserToken<Guid>>().Where(ut => ut.UserId == userId).ExecuteDeleteAsync(cancellationToken);

            // Delete the ApplicationUser row directly
            await _context.ApplicationUsers.IgnoreQueryFilters()
                .Where(u => u.Id == userId)
                .ExecuteDeleteAsync(cancellationToken);

            foreach (var (path, place, isVideo, isImage) in filesToDelete)
            {
                try
                {
                    if (isImage)
                    {
                        await _imageValidator.DeleteImage(path, place);
                    }
                    else if (isVideo)
                    {
                        await _videoValidator.DeleteVideo(path, place);
                    }
                    else
                    {
                        await _fileValidator.DeleteFile(path, place);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to delete file {Path} for user {UserId}", path, userId);
                }
            }
        }
    }
}

