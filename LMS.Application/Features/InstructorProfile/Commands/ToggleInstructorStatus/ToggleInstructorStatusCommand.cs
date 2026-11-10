using LMS.Application.Common.Exceptions;
using LMS.Application.Common.Interfaces;
using LMS.Application.Common.Models;
using LMS.Application.Localization;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace LMS.Application.Features.InstructorProfile.Commands.ToggleInstructorStatus
{
    public class ToggleInstructorStatusCommand : IRequest<Result<bool>>
    {
        public Guid InstructorId { get; set; }
    }

    public class ToggleInstructorStatusCommandHandler : IRequestHandler<ToggleInstructorStatusCommand, Result<bool>>
    {
        private readonly ILmsDbContext _context;
        private readonly IStringLocalizer<Messages> _localizer;

        public ToggleInstructorStatusCommandHandler(
            ILmsDbContext context,
            IStringLocalizer<Messages> localizer)
        {
            _context = context;
            _localizer = localizer;
        }

        public async Task<Result<bool>> Handle(
            ToggleInstructorStatusCommand request,
            CancellationToken cancellationToken)
        {
            var instructor = await _context.Instructors
                .Include(i => i.User)
                .FirstOrDefaultAsync(i => i.Id == request.InstructorId, cancellationToken)
                ?? throw new NotFoundException(LocalizationKeys.Availability.InstructorNotFound);

            var user = instructor.User;
            user.IsActive = !user.IsActive;
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            return Result<bool>.Success(
                user.IsActive,
                _localizer[LocalizationKeys.AdminManagement.TeacherStatusUpdatedSuccess].Value);
        }
    }
}
