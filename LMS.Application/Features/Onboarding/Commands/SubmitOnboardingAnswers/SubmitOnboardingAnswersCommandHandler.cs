using LMS.API.Services;
using LMS.Application.Common.Exceptions;
using LMS.Application.Common.Interfaces;
using LMS.Application.Common.Models;
using LMS.Application.Localization;
using LMS.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace LMS.Application.Features.Onboarding.Commands.SubmitOnboardingAnswers
{
    public class SubmitOnboardingAnswersCommandHandler
        : IRequestHandler<SubmitOnboardingAnswersCommand, Result<string>>
    {
        private readonly ILmsDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly IStringLocalizer<Messages> _localizer;

        public SubmitOnboardingAnswersCommandHandler(
            ILmsDbContext context,
            ICurrentUserService currentUserService,
            IStringLocalizer<Messages> localizer)
        {
            _context = context;
            _currentUserService = currentUserService;
            _localizer = localizer;
        }

        public async Task<Result<string>> Handle(
            SubmitOnboardingAnswersCommand request,
            CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;
            if (userId == Guid.Empty)
            {
                throw new UnAuthorizedException();
            }

            var answers = request.Answers ?? new List<OnboardingAnswerInput>();
            var questionIds = answers.Select(answer => answer.QuestionId).ToArray();

            var existingAnswers = await _context.OnboardingAnswers
                .Where(answer => answer.UserId == userId && questionIds.Contains(answer.OnboardingQuestionId))
                .ToListAsync(cancellationToken);

            foreach (var existing in existingAnswers)
            {
                existing.MarkAsDeleted(userId.ToString());
            }

            foreach (var answerInput in answers)
            {
                var optionIds = answerInput.OptionIds ?? new List<Guid>();

                if (optionIds.Count > 0)
                {
                    foreach (var optionId in optionIds)
                    {
                        await _context.OnboardingAnswers.AddAsync(new OnboardingAnswer
                        {
                            UserId = userId,
                            OnboardingQuestionId = answerInput.QuestionId,
                            OnboardingOptionId = optionId
                        }, cancellationToken);
                    }
                }
                else
                {
                    await _context.OnboardingAnswers.AddAsync(new OnboardingAnswer
                    {
                        UserId = userId,
                        OnboardingQuestionId = answerInput.QuestionId,
                        NumericValue = answerInput.NumericValue
                    }, cancellationToken);
                }
            }

            await _context.SaveChangesAsync(cancellationToken);

            var message = _localizer[LocalizationKeys.Onboarding.AnswersSubmittedSuccess].Value;
            return Result<string>.Success(message, message);
        }
    }
}
