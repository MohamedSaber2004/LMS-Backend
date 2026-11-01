using LMS.Application.Common.Interfaces;
using LMS.Application.Common.Models;
using LMS.Application.Features.Onboarding.Commands.Admin;
using LMS.Application.Features.Onboarding.DTOs;
using LMS.Application.Localization;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace LMS.Application.Features.Onboarding.Queries.GetActiveOnboardingQuestions
{
    public class GetActiveOnboardingQuestionsQueryHandler
        : IRequestHandler<GetActiveOnboardingQuestionsQuery, Result<IReadOnlyCollection<OnboardingQuestionDto>>>
    {
        private readonly ILmsDbContext _context;
        private readonly IStringLocalizer<Messages> _localizer;

        public GetActiveOnboardingQuestionsQueryHandler(
            ILmsDbContext context,
            IStringLocalizer<Messages> localizer)
        {
            _context = context;
            _localizer = localizer;
        }

        public async Task<Result<IReadOnlyCollection<OnboardingQuestionDto>>> Handle(
            GetActiveOnboardingQuestionsQuery request,
            CancellationToken cancellationToken)
        {
            var questions = await _context.OnboardingQuestions
                .AsNoTracking()
                .Include(item => item.Options)
                .Where(item => item.IsActive)
                .OrderBy(item => item.Order)
                .ToListAsync(cancellationToken);

            var items = questions
                .Select(question => OnboardingMapper.ToDto(question, onlyActiveOptions: true))
                .ToArray();

            return Result<IReadOnlyCollection<OnboardingQuestionDto>>.Success(
                items,
                _localizer[LocalizationKeys.Onboarding.QuestionsRetrieved].Value);
        }
    }
}
