using LMS.API.Services;
using LMS.Application.Common.Exceptions;
using LMS.Application.Common.Interfaces;
using LMS.Application.Common.Models;
using LMS.Application.Features.Sessions.DTOs;
using LMS.Application.Localization;
using LMS.Domain.Entities;
using LMS.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace LMS.Application.Features.Sessions.Commands.RequestReschedule
{
    public class RequestRescheduleCommandHandler
        : IRequestHandler<RequestRescheduleCommand, Result<RescheduleRequestDto>>
    {
        private readonly ILmsDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly IStringLocalizer<Messages> _localizer;

        public RequestRescheduleCommandHandler(
            ILmsDbContext context,
            ICurrentUserService currentUserService,
            IStringLocalizer<Messages> localizer)
        {
            _context = context;
            _currentUserService = currentUserService;
            _localizer = localizer;
        }

        public async Task<Result<RescheduleRequestDto>> Handle(
            RequestRescheduleCommand request,
            CancellationToken cancellationToken)
        {
            var session = await _context.Sessions
                .FirstOrDefaultAsync(item => item.Id == request.Id, cancellationToken)
                ?? throw new NotFoundException(LocalizationKeys.Sessions.SessionNotFound);

            var actor = await SessionGuards.ResolveActorAsync(
                _context, _currentUserService, session, cancellationToken);

            var reschedule = RescheduleRequest.Create(
                session.Id,
                _currentUserService.UserId,
                actor == SessionActor.Teacher ? UserType.Instructor : UserType.Learner,
                request.FromUtc,
                request.ToUtc);

            await _context.RescheduleRequests.AddAsync(reschedule, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            return Result<RescheduleRequestDto>.Success(
                SessionMapper.ToRescheduleDto(reschedule),
                _localizer[LocalizationKeys.Sessions.RescheduleRequestedSuccess].Value,
                StatusCodes.Status201Created);
        }
    }
}
