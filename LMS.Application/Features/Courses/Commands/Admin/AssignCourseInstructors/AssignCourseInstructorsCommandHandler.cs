using LMS.API.Services;
using LMS.Application.Common.Interfaces;
using LMS.Application.Common.Models;
using LMS.Application.Features.Courses.DTOs;
using LMS.Application.Localization;
using LMS.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace LMS.Application.Features.Courses.Commands.Admin.AssignCourseInstructors
{
    public class AssignCourseInstructorsCommandHandler
        : IRequestHandler<AssignCourseInstructorsCommand, Result<IReadOnlyCollection<InstructorRefDto>>>
    {
        private readonly ILmsDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly IStringLocalizer<Messages> _localizer;

        public AssignCourseInstructorsCommandHandler(
            ILmsDbContext context,
            ICurrentUserService currentUserService,
            IStringLocalizer<Messages> localizer)
        {
            _context = context;
            _currentUserService = currentUserService;
            _localizer = localizer;
        }

        public async Task<Result<IReadOnlyCollection<InstructorRefDto>>> Handle(
            AssignCourseInstructorsCommand request,
            CancellationToken cancellationToken)
        {
            var desiredIds = request.InstructorIds.Distinct().ToArray();

            var currentRows = await _context.CourseInstructors
                .Where(row => row.CourseId == request.Id)
                .ToListAsync(cancellationToken);

            var removedBy = _currentUserService.UserId.ToString();

            foreach (var row in currentRows.Where(row => !desiredIds.Contains(row.InstructorId)))
            {
                row.MarkAsDeleted(removedBy);
            }

            foreach (var instructorId in desiredIds.Except(currentRows.Select(row => row.InstructorId)))
            {
                await _context.CourseInstructors.AddAsync(
                    CourseInstructor.Create(request.Id, instructorId),
                    cancellationToken);
            }

            await _context.SaveChangesAsync(cancellationToken);

            var instructors = await CoursesInstructors.ForCourseAsync(
                _context, request.Id, cancellationToken);

            return Result<IReadOnlyCollection<InstructorRefDto>>.Success(
                instructors,
                _localizer[LocalizationKeys.Courses.InstructorsAssignedSuccess].Value);
        }
    }
}
