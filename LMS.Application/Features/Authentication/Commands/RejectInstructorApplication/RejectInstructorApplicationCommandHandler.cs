using LMS.API.Services;
using LMS.Application.Common.Exceptions;
using LMS.Application.Common.Interfaces;
using LMS.Application.Common.Models;
using LMS.Application.Localization;
using LMS.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace LMS.Application.Features.Authentication.Commands.RejectInstructorApplication
{
    public class RejectInstructorApplicationCommandHandler
        : IRequestHandler<RejectInstructorApplicationCommand, Result<string>>
    {
        private readonly ILmsDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly IStringLocalizer<Messages> _localizer;

        public RejectInstructorApplicationCommandHandler(
            ILmsDbContext context,
            ICurrentUserService currentUserService,
            IStringLocalizer<Messages> localizer)
        {
            _context = context;
            _currentUserService = currentUserService;
            _localizer = localizer;
        }

        public async Task<Result<string>> Handle(
            RejectInstructorApplicationCommand request,
            CancellationToken cancellationToken)
        {
            var adminUserId = _currentUserService.UserId;
            if (adminUserId == Guid.Empty)
            {
                throw new UnAuthorizedException();
            }

            var application = await _context.InstructorApplications
                .FirstOrDefaultAsync(item => item.Id == request.ApplicationId, cancellationToken)
                ?? throw new NotFoundException(
                    LocalizationKeys.Authentication.InstructorApplicationNotFound);

            if (application.Status != InstructorApplicationStatus.Pending)
            {
                throw new BadRequestException(
                    LocalizationKeys.Authentication.InstructorApplicationAlreadyReviewed);
            }

            application.Reject(adminUserId, request.ReviewNotes.Trim());
            await _context.SaveChangesAsync(cancellationToken);

            var message = _localizer[LocalizationKeys.Authentication.InstructorRejectedSuccess].Value;
            return Result<string>.Success(message, message);
        }
    }
}
