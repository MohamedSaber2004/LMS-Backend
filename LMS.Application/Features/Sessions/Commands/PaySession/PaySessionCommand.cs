using LMS.API.Services;
using LMS.Application.Common.Exceptions;
using LMS.Application.Common.Helpers;
using LMS.Application.Common.Interfaces;
using LMS.Application.Common.Models;
using LMS.Application.Features.Sessions.DTOs;
using LMS.Application.Localization;
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
                ?? throw new ForbiddenException(LocalizationKeys.ExceptionMessages.Forbidden);

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

            if (session.Status != SessionStatus.Confirmed)
            {
                throw new BadRequestException(LocalizationKeys.Sessions.SessionMustBeConfirmedBeforePayment);
            }

            if (session.IsPaid)
            {
                throw new BadRequestException(LocalizationKeys.Sessions.PaymentAlreadyCompleted);
            }

            if (!session.IsTrial)
            {
                var requiredHours = session.DurationMinutes == 25 ? 0.5m : 1.0m;
                var package = await _context.LearnerPackages
                    .Where(lp => lp.LearnerId == learnerId
                        && lp.IsActive
                        && lp.RemainingHours >= requiredHours)
                    .OrderBy(lp => lp.CreatedAt)
                    .FirstOrDefaultAsync(cancellationToken)
                    ?? throw new BadRequestException(LocalizationKeys.Sessions.InsufficientPackageHours);

                package.DeductHours(requiredHours);
            }

            session.MarkPaid(DateTime.UtcNow);
            await _context.SaveChangesAsync(cancellationToken);

            var dto = SessionMapper.ToDto(session, session.Instructor.User?.FullName ?? string.Empty, booking);
            return Result<SessionDto>.Success(dto, _localizer[LocalizationKeys.Sessions.PaymentSuccess].Value);
        }
    }
}

