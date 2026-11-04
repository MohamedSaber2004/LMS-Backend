using LMS.API.Services;
using LMS.Application.Common.Exceptions;
using LMS.Application.Common.Interfaces;
using LMS.Application.Common.Models;
using LMS.Application.Features.Sessions.DTOs;
using LMS.Application.Localization;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace LMS.Application.Features.Sessions.Commands.DeclineReschedule
{
    public class DeclineRescheduleCommandHandler
        : IRequestHandler<DeclineRescheduleCommand, Result<RescheduleRequestDto>>
    {
        private readonly ILmsDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly IStringLocalizer<Messages> _localizer;

        public DeclineRescheduleCommandHandler(
            ILmsDbContext context,
            ICurrentUserService currentUserService,
            IStringLocalizer<Messages> localizer)
        {
            _context = context;
            _currentUserService = currentUserService;
            _localizer = localizer;
        }

        public async Task<Result<RescheduleRequestDto>> Handle(
            DeclineRescheduleCommand request,
            CancellationToken cancellationToken)
        {
            var reschedule = await _context.RescheduleRequests
                .FirstOrDefaultAsync(item => item.Id == request.Id, cancellationToken)
                ?? throw new NotFoundException(LocalizationKeys.Sessions.RescheduleNotFound);

            // Refusing leaves the session exactly where it was: a decline touches only the request.
            reschedule.Decline(_currentUserService.UserId);

            await _context.SaveChangesAsync(cancellationToken);

            return Result<RescheduleRequestDto>.Success(
                SessionMapper.ToRescheduleDto(reschedule),
                _localizer[LocalizationKeys.Sessions.RescheduleDeclinedSuccess].Value);
        }
    }
}
