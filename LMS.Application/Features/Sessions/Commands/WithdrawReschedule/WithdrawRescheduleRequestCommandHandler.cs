using LMS.API.Services;
using LMS.Application.Common.Exceptions;
using LMS.Application.Common.Interfaces;
using LMS.Application.Common.Models;
using LMS.Application.Features.Sessions.DTOs;
using LMS.Application.Localization;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace LMS.Application.Features.Sessions.Commands.WithdrawReschedule
{
    public class WithdrawRescheduleRequestCommandHandler
        : IRequestHandler<WithdrawRescheduleRequestCommand, Result<RescheduleRequestDto>>
    {
        private readonly ILmsDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly IStringLocalizer<Messages> _localizer;

        public WithdrawRescheduleRequestCommandHandler(
            ILmsDbContext context,
            ICurrentUserService currentUserService,
            IStringLocalizer<Messages> localizer)
        {
            _context = context;
            _currentUserService = currentUserService;
            _localizer = localizer;
        }

        public async Task<Result<RescheduleRequestDto>> Handle(
            WithdrawRescheduleRequestCommand request,
            CancellationToken cancellationToken)
        {
            var reschedule = await _context.RescheduleRequests
                .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken)
                ?? throw new NotFoundException(LocalizationKeys.Sessions.RescheduleNotFound);

            reschedule.Withdraw(_currentUserService.UserId);
            await _context.SaveChangesAsync(cancellationToken);

            return Result<RescheduleRequestDto>.Success(
                SessionMapper.ToRescheduleDto(reschedule),
                _localizer[LocalizationKeys.Sessions.RescheduleWithdrawnSuccess].Value);
        }
    }
}

