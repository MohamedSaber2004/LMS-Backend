using LMS.API.Services;
using LMS.Application.Common.Exceptions;
using LMS.Application.Common.Interfaces;
using LMS.Application.Common.Models;
using LMS.Application.Features.InstructorProfile.DTOs;
using LMS.Application.Localization;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace LMS.Application.Features.InstructorProfile.Queries.GetInstructorProfile
{
    public class GetInstructorProfileQueryHandler
        : IRequestHandler<GetInstructorProfileQuery, Result<InstructorProfileDto>>
    {
        private readonly ILmsDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly IStringLocalizer<Messages> _localizer;

        public GetInstructorProfileQueryHandler(
            ILmsDbContext context,
            ICurrentUserService currentUserService,
            IStringLocalizer<Messages> localizer)
        {
            _context = context;
            _currentUserService = currentUserService;
            _localizer = localizer;
        }


        public async Task<Result<InstructorProfileDto>> Handle(
            GetInstructorProfileQuery request,
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
                .FirstOrDefaultAsync(item => item.UserId == userId, cancellationToken)
                ?? throw new NotFoundException(LocalizationKeys.Authentication.UserNotFound);

            return Result<InstructorProfileDto>.Success(
                InstructorProfileMapper.ToDto(instructor),
                _localizer[LocalizationKeys.InstructorProfile.ProfileRetrieved].Value);
        }
    }
}
