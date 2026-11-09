using LMS.Application.Common.Interfaces;
using LMS.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LMS.Infrastructure.Services.Scheduling
{
    public class VacationBackgroundService : IVacationBackgroundService
    {
        private readonly ILmsDbContext _context;
        private readonly ILogger<VacationBackgroundService> _logger;

        public VacationBackgroundService(
            ILmsDbContext context,
            ILogger<VacationBackgroundService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task ProcessVacationConflictsAsync(Guid instructorId, CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;

            var conflictedSessions = await _context.Sessions
                .Include(s => s.Bookings)
                    .ThenInclude(b => b.Learner)
                        .ThenInclude(l => l.User)
                .Where(s => s.InstructorId == instructorId
                    && (s.Status == SessionStatus.Confirmed || s.Status == SessionStatus.Pending)
                    && s.EndsAtUtc > now)
                .OrderBy(s => s.StartsAtUtc)
                .ToListAsync(cancellationToken);

            if (conflictedSessions.Count == 0)
            {
                _logger.LogInformation("No conflicted sessions found for instructor {InstructorId} upon vacation activation.", instructorId);
                return;
            }

            _logger.LogWarning(
                "Vacation mode enabled for instructor {InstructorId}. Background processor detected {Count} upcoming active session(s).",
                instructorId,
                conflictedSessions.Count);

            foreach (var session in conflictedSessions)
            {
                _logger.LogInformation(
                    "Session {SessionId} starting at {StartsAt} overlaps with instructor {InstructorId} vacation mode.",
                    session.Id,
                    session.StartsAtUtc,
                    instructorId);
            }

            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}

