using LMS.API.Services;
using LMS.Application.Common.Exceptions;
using LMS.Application.Common.Interfaces;
using LMS.Application.Common.Models;
using LMS.Application.Features.Onboarding.DTOs;
using LMS.Application.Localization;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace LMS.Application.Features.Onboarding.Commands.Admin.UpdateOnboardingQuestion
{
    public class UpdateOnboardingQuestionCommandHandler
        : IRequestHandler<UpdateOnboardingQuestionCommand, Result<OnboardingQuestionDto>>
    {
        private readonly ILmsDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly IStringLocalizer<Messages> _localizer;

        public UpdateOnboardingQuestionCommandHandler(
            ILmsDbContext context,
            ICurrentUserService currentUserService,
            IStringLocalizer<Messages> localizer)
        {
            _context = context;
            _currentUserService = currentUserService;
            _localizer = localizer;
        }

        public async Task<Result<OnboardingQuestionDto>> Handle(
            UpdateOnboardingQuestionCommand request,
            CancellationToken cancellationToken)
        {
            var question = await _context.OnboardingQuestions
                .Include(item => item.Options)
                .FirstOrDefaultAsync(item => item.Id == request.Id, cancellationToken)
                ?? throw new NotFoundException(LocalizationKeys.Onboarding.QuestionNotFound);

            question.Title = request.Title.Trim();
            question.Order = request.Order;
            question.MinValue = request.MinValue;
            question.MaxValue = request.MaxValue;
            question.Step = request.Step;
            question.SetActiveState(request.IsActive, _currentUserService.UserId.ToString());

            await _context.SaveChangesAsync(cancellationToken);

            var response = OnboardingMapper.ToDto(question);

            return Result<OnboardingQuestionDto>.Success(
                response,
                _localizer[LocalizationKeys.Onboarding.QuestionUpdatedSuccess].Value);
        }
    }
}
