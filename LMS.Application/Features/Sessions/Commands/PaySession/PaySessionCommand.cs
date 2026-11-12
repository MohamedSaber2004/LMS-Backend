using LMS.API.Services;
using LMS.Application.Common.Exceptions;
using LMS.Application.Common.Helpers;
using LMS.Application.Common.Interfaces;
using LMS.Application.Common.Models;
using LMS.Application.Features.Sessions.DTOs;
using LMS.Application.Localization;
using LMS.Domain.Entities;
using LMS.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace LMS.Application.Features.Sessions.Commands.PaySession
{
    public class PaySessionCommand : IRequest<Result<SessionDto>>
    {
        public Guid SessionId { get; set; }
    }

    public class PaySessionCommandHandler : IRequestHandler<PaySessionCommand, Result<SessionDto>>
    {
        private readonly ILmsDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly IStringLocalizer<Messages> _localizer;

        public PaySessionCommandHandler(
            ILmsDbContext context,
            ICurrentUserService currentUserService,
            IStringLocalizer<Messages> localizer)
        {
            _context = context;
            _currentUserService = currentUserService;
            _localizer = localizer;
        }

        public async Task<Result<SessionDto>> Handle(
            PaySessionCommand request,
            CancellationToken cancellationToken)
        {
            var learnerId = await ActorOwnership.ResolveLearnerIdAsync(
                    _context, _currentUserService.UserId, cancellationToken)
                ?? throw new ForbiddenException(
                    LocalizationKeys.ExceptionMessages.LearnerProfileMissing, _currentUserService.UserId);

            var session = await _context.Sessions
                .Include(s => s.Instructor)
                    .ThenInclude(i => i.User)
                .Include(s => s.Bookings)
                .FirstOrDefaultAsync(s => s.Id == request.SessionId, cancellationToken)
                ?? throw new NotFoundException(LocalizationKeys.Sessions.SessionNotFound);

            var booking = session.Bookings.FirstOrDefault(b => b.LearnerId == learnerId);
            if (booking == null)
            {
                throw new ForbiddenException(LocalizationKeys.ExceptionMessages.Forbidden);
            }

            if (session.IsPaid)
            {
                throw new BadRequestException(LocalizationKeys.Sessions.PaymentAlreadyCompleted);
            }

            if (session.Status != SessionStatus.Pending)
            {
                throw new BadRequestException(
                    LocalizationKeys.Sessions.SessionNotPayable, session.Status);
            }

            if (session.StartsAtUtc <= DateTime.UtcNow)
            {
                throw new BadRequestException(
                    LocalizationKeys.Sessions.PaymentWindowPassed,
                    session.StartsAtUtc.ToString("yyyy-MM-dd HH:mm"));
            }

            var requiredHours = session.DurationMinutes == 25 ? 0.5m : 1.0m;
            var package = await _context.LearnerPackages
                .Where(lp => lp.LearnerId == learnerId
                    && lp.IsActive
                    && lp.RemainingHours >= requiredHours)
                .OrderBy(lp => lp.CreatedAt)
                .FirstOrDefaultAsync(cancellationToken)
                ?? throw new BadRequestException(LocalizationKeys.Sessions.InsufficientPackageHours);

            package.DeductHours(requiredHours);

            session.Confirm();
            booking.Confirm();
            session.MarkPaid(DateTime.UtcNow);

            var grossAmount = session.Price > 0
                ? session.Price
                : Math.Round(session.HourlyRateSnapshot * requiredHours, 2);

            if (grossAmount > 0)
            {
                var wallet = await _context.TeacherWallets
                    .Include(w => w.Transactions)
                    .FirstOrDefaultAsync(w => w.InstructorId == session.InstructorId, cancellationToken);

                if (wallet == null)
                {
                    wallet = TeacherWallet.Create(session.InstructorId);
                    await _context.TeacherWallets.AddAsync(wallet, cancellationToken);
                }

                var commissionRate = session.CommissionRateSnapshot > 0
                    ? session.CommissionRateSnapshot
                    : session.Instructor.CommissionRate;

                wallet.HoldSessionEarnings(
                    session.Id,
                    grossAmount,
                    commissionRate,
                    $"Held earnings for session on {session.StartsAtUtc:yyyy-MM-dd HH:mm} UTC");
            }

            await _context.SaveChangesAsync(cancellationToken);

            var dto = SessionMapper.ToDto(session, session.Instructor.User?.FullName ?? string.Empty, booking);
            return Result<SessionDto>.Success(dto, _localizer[LocalizationKeys.Sessions.PaymentSuccess].Value);
        }
    }
}
