using LMS.API.Services;
using LMS.Application.Common.Exceptions;
using LMS.Application.Common.Interfaces;
using LMS.Application.Common.Models;
using LMS.Application.Features.Authentication.DTOs;
using LMS.Application.Localization;
using LMS.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace LMS.Application.Features.Authentication.Queries.GetMyInstructorApplication
{
    public class GetMyInstructorApplicationQueryHandler
        : IRequestHandler<GetMyInstructorApplicationQuery, Result<MyInstructorApplicationDto>>
    {
        private readonly ILmsDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly IStringLocalizer<Messages> _localizer;

        public GetMyInstructorApplicationQueryHandler(
            ILmsDbContext context,
            ICurrentUserService currentUserService,
            IStringLocalizer<Messages> localizer)
        {
            _context = context;
            _currentUserService = currentUserService;
            _localizer = localizer;
        }

        public async Task<Result<MyInstructorApplicationDto>> Handle(
            GetMyInstructorApplicationQuery request,
            CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;
            if (userId == Guid.Empty)
            {
                throw new UnAuthorizedException();
            }

            var instructor = await _context.Instructors
                .AsNoTracking()
                .Include(item => item.User)
                .Include(item => item.Application)
                .Include(item => item.Certificates)
                .FirstOrDefaultAsync(item => item.UserId == userId, cancellationToken);

            if (instructor?.Application is null)
            {
                throw new NotFoundException(
                    LocalizationKeys.Authentication.InstructorApplicationNotFound);
            }

            var application = instructor.Application;

            var response = new MyInstructorApplicationDto(
                application.Id,
                application.Status,
                instructor.HourlyRate,
                instructor.User.IsActive,
                instructor.User.EmailConfirmed,
                application.CreatedAt,
                application.ReviewedAt,
                application.ReviewNotes,
                instructor.Certificates.Count(item => item.Type != InstructorCertificateType.IntroVideo),
                instructor.Certificates.Any(item => item.Type == InstructorCertificateType.IntroVideo));

            return Result<MyInstructorApplicationDto>.Success(
                response,
                _localizer[LocalizationKeys.Authentication.MyInstructorApplicationRetrieved].Value);
        }
    }
}
