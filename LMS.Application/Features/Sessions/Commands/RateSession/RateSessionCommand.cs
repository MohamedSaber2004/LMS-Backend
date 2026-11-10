using FluentValidation;
using LMS.API.Services;
using LMS.Application.Common.Exceptions;
using LMS.Application.Common.Helpers;
using LMS.Application.Common.Interfaces;
using LMS.Application.Common.Models;
using LMS.Application.Localization;
using LMS.Domain.Entities;
using LMS.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace LMS.Application.Features.Sessions.Commands.RateSession
{
    public class RateSessionCommand : IRequest<Result<string>>
    {
        public Guid SessionId { get; set; }
        public int Rating { get; set; } // 1 to 5
        public string? Comment { get; set; }
    }

    public class RateSessionCommandValidator : AbstractValidator<RateSessionCommand>
    {
        public RateSessionCommandValidator(IStringLocalizer<Messages> localizer)
        {
            RuleFor(x => x.SessionId)
                .NotEmpty()
                .WithMessage(localizer[LocalizationKeys.Reviews.SessionIdRequired].Value);

            RuleFor(x => x.Rating)
                .InclusiveBetween(1, 5)
                .WithMessage(localizer[LocalizationKeys.Reviews.RatingRangeInvalid].Value);

            RuleFor(x => x.Comment)
                .MaximumLength(1000)
                .WithMessage(localizer[LocalizationKeys.Reviews.CommentTooLong].Value);
        }
    }

    public class RateSessionCommandHandler : IRequestHandler<RateSessionCommand, Result<string>>
    {
        private readonly ILmsDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly IStringLocalizer<Messages> _localizer;

        public RateSessionCommandHandler(
            ILmsDbContext context,
            ICurrentUserService currentUserService,
            IStringLocalizer<Messages> localizer)
        {
            _context = context;
            _currentUserService = currentUserService;
            _localizer = localizer;
        }

        public async Task<Result<string>> Handle(
            RateSessionCommand request,
            CancellationToken cancellationToken)
        {
            var learnerId = await ActorOwnership.ResolveLearnerIdAsync(
                    _context, _currentUserService.UserId, cancellationToken)
                ?? throw new ForbiddenException(LocalizationKeys.ExceptionMessages.Forbidden);

            var session = await _context.Sessions
                .Include(s => s.Bookings)
                .Include(s => s.Instructor)
                .FirstOrDefaultAsync(s => s.Id == request.SessionId, cancellationToken)
                ?? throw new NotFoundException(LocalizationKeys.Sessions.SessionNotFound);

            var booking = session.Bookings.FirstOrDefault(b => b.LearnerId == learnerId);
            if (booking == null)
            {
                throw new ForbiddenException(LocalizationKeys.ExceptionMessages.Forbidden);
            }

            if (session.Status != SessionStatus.Completed && !session.IsAttendanceConfirmed)
            {
                throw new BadRequestException(LocalizationKeys.Reviews.CannotReviewIncomplete);
            }

            var existingReview = await _context.Set<SessionReview>()
                .FirstOrDefaultAsync(r => r.SessionId == session.Id, cancellationToken);

            if (existingReview != null)
            {
                throw new BadRequestException(LocalizationKeys.Reviews.AlreadyReviewed);
            }

            var review = SessionReview.Create(
                session.Id,
                learnerId,
                session.InstructorId,
                request.Rating,
                request.Comment);

            await _context.Set<SessionReview>().AddAsync(review, cancellationToken);
            session.Instructor.RecordSessionReview(request.Rating);
            session.Instructor.IncrementCompletedSessions();

            await _context.SaveChangesAsync(cancellationToken);

            var message = _localizer[LocalizationKeys.Reviews.ReviewSuccess].Value;
            return Result<string>.Success(message, message);
        }
    }
}

