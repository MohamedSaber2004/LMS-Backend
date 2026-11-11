using LMS.Application.Common.Exceptions;
using LMS.Application.Common.Interfaces;
using LMS.Application.Common.Models;
using LMS.Application.Localization;
using LMS.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace LMS.Application.Features.Sessions.Commands.AdminCancelSession
{
    public class AdminCancelSessionCommand : IRequest<Result<string>>
    {
        public Guid SessionId { get; set; }
        public string? Reason { get; set; }
    }

    public class AdminCancelSessionCommandHandler : IRequestHandler<AdminCancelSessionCommand, Result<string>>
    {
        private readonly ILmsDbContext _context;
        private readonly IStringLocalizer<Messages> _localizer;

        public AdminCancelSessionCommandHandler(
            ILmsDbContext context,
            IStringLocalizer<Messages> localizer)
        {
            _context = context;
            _localizer = localizer;
        }

        public async Task<Result<string>> Handle(
            AdminCancelSessionCommand request,
            CancellationToken cancellationToken)
        {
            var session = await _context.Sessions
                .Include(s => s.Bookings)
                .FirstOrDefaultAsync(s => s.Id == request.SessionId, cancellationToken)
                ?? throw new NotFoundException(LocalizationKeys.Sessions.SessionNotFound);

            if (session.Status == SessionStatus.Cancelled)
            {
                throw new BadRequestException(LocalizationKeys.Sessions.SessionNotCancellable);
            }

            session.Cancel(null, request.Reason ?? "Cancelled by platform administrator.");

            if (!session.IsTrial && session.IsPaid)
            {
                var booking = session.Bookings.FirstOrDefault();
                if (booking != null)
                {
                    var hoursToRefund = session.DurationMinutes == 25 ? 0.5m : 1.0m;
                    var package = await _context.LearnerPackages
                        .Where(lp => lp.LearnerId == booking.LearnerId)
                        .OrderByDescending(lp => lp.CreatedAt)
                        .FirstOrDefaultAsync(cancellationToken);

                    if (package != null)
                    {
                        package.RefundHours(hoursToRefund);
                    }
                }
            }

            await _context.SaveChangesAsync(cancellationToken);

            return Result<string>.Success(
                "Session cancelled successfully by administrator.",
                _localizer[LocalizationKeys.AdminManagement.SessionCancelledSuccess].Value);
        }
    }
}
