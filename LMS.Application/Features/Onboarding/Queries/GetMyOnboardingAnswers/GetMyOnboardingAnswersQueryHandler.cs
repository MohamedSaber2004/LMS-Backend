using LMS.API.Services;
using LMS.Application.Common.Exceptions;
using LMS.Application.Common.Interfaces;
using LMS.Application.Common.Models;
using LMS.Application.Features.Onboarding.DTOs;
using LMS.Application.Localization;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace LMS.Application.Features.Onboarding.Queries.GetMyOnboardingAnswers
{
    public class GetMyOnboardingAnswersQueryHandler
        : IRequestHandler<GetMyOnboardingAnswersQuery, Result<IReadOnlyCollection<OnboardingAnswerGroupDto>>>
    {
        private readonly ILmsDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly IStringLocalizer<Messages> _localizer;

        public GetMyOnboardingAnswersQueryHandler(
            ILmsDbContext context,
            ICurrentUserService currentUserService,
            IStringLocalizer<Messages> localizer)
        {
            _context = context;
            _currentUserService = currentUserService;
            _localizer = localizer;
        }

        public async Task<Result<IReadOnlyCollection<OnboardingAnswerGroupDto>>> Handle(
            GetMyOnboardingAnswersQuery request,
            CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;
            if (userId == Guid.Empty)
            {
                throw new UnAuthorizedException();
            }

            var answers = await _context.OnboardingAnswers
                .AsNoTracking()
                .Include(answer => answer.OnboardingQuestion)
                .Include(answer => answer.OnboardingOption)
                .Where(answer => answer.UserId == userId)
                .OrderBy(answer => answer.OnboardingQuestion.Order)
                .ThenBy(answer => answer.CreatedAt)
                .ToListAsync(cancellationToken);

            var items = answers
                .GroupBy(answer => answer.OnboardingQuestionId)
                .Select(group => new OnboardingAnswerGroupDto
                {
                    QuestionId = group.Key,
                    Key = group.First().OnboardingQuestion.Key,
                    Title = group.First().OnboardingQuestion.Title,
                    Answers = group.Select(answer => new OnboardingAnswerDto
                    {
                        Id = answer.Id,
                        OptionId = answer.OnboardingOptionId,
                        OptionLabel = answer.OnboardingOption?.Label,
                        OptionValue = answer.OnboardingOption?.Value,
                        NumericValue = answer.NumericValue
                    }).ToArray()
                })
                .ToArray();

            return Result<IReadOnlyCollection<OnboardingAnswerGroupDto>>.Success(
                items,
                _localizer[LocalizationKeys.Onboarding.AnswersRetrieved].Value);
        }
    }
}
