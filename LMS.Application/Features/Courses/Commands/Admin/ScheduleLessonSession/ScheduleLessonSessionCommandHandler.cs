using LMS.Application.Common.Exceptions;
using LMS.Application.Common.Interfaces;
using LMS.Application.Common.Models;
using LMS.Application.Features.Courses.DTOs;
using LMS.Application.Localization;
using LMS.Domain.Entities;
using LMS.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace LMS.Application.Features.Courses.Commands.Admin.ScheduleLessonSession
{
    public class ScheduleLessonSessionCommandHandler
        : IRequestHandler<ScheduleLessonSessionCommand, Result<LessonAdminDto>>
    {
        private readonly ILmsDbContext _context;
        private readonly IStringLocalizer<Messages> _localizer;

        public ScheduleLessonSessionCommandHandler(
            ILmsDbContext context,
            IStringLocalizer<Messages> localizer)
        {
            _context = context;
            _localizer = localizer;
        }

        public async Task<Result<LessonAdminDto>> Handle(
            ScheduleLessonSessionCommand request,
            CancellationToken cancellationToken)
        {
            var lesson = await _context.Lessons
                .Include(l => l.Course)
                    .ThenInclude(c => c.Enrollments)
                .Include(l => l.Session)
                    .ThenInclude(s => s!.Bookings)
                .Include(l => l.Session)
                    .ThenInclude(s => s!.LiveRoom)
                .Include(l => l.QuizQuestions)
                    .ThenInclude(q => q.Options)
                .FirstOrDefaultAsync(l => l.Id == request.LessonId, cancellationToken)
                ?? throw new NotFoundException(LocalizationKeys.Courses.LessonNotFound);

            var instructorExists = await _context.Instructors
                .AnyAsync(i => i.Id == request.InstructorId, cancellationToken);

            if (!instructorExists)
            {
                throw new NotFoundException(LocalizationKeys.Availability.InstructorNotFound);
            }

            Session session;

            if (lesson.Session is not null)
            {
                session = lesson.Session;
                session.MoveTo(request.StartsAtUtc, request.EndsAtUtc);
                if (session.Status != SessionStatus.Confirmed)
                {
                    session.Confirm();
                }
            }
            else
            {
                session = Session.Create(
                    request.InstructorId,
                    SessionType.Group,
                    request.StartsAtUtc,
                    request.EndsAtUtc,
                    0,
                    lesson.Title,
                    request.SeatCapacity,
                    lesson.Id);

                session.Confirm();
                await _context.Sessions.AddAsync(session, cancellationToken);
            }

            // Automatically book seats for all already-enrolled learners
            foreach (var enrollment in lesson.Course.Enrollments)
            {
                var hasBooking = session.Bookings.Any(b =>
                    b.LearnerId == enrollment.LearnerId &&
                    b.Status != BookingStatus.Cancelled &&
                    b.Status != BookingStatus.Declined);

                if (!hasBooking)
                {
                    var booking = SessionBooking.Create(session.Id, enrollment.LearnerId);
                    booking.Confirm();
                    await _context.SessionBookings.AddAsync(booking, cancellationToken);
                    session.ReserveSeat();
                }
            }

            await _context.SaveChangesAsync(cancellationToken);

            return Result<LessonAdminDto>.Success(
                CoursesMapper.ToAdminLessonDto(lesson),
                _localizer[LocalizationKeys.Courses.LessonSessionScheduledSuccess].Value);
        }
    }
}
