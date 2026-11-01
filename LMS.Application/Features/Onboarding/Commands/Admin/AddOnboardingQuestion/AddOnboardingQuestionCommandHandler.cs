using LMS.Application.Common.Interfaces;
using LMS.Application.Common.Models;
using LMS.Application.Features.Onboarding.DTOs;
using LMS.Application.Localization;
using LMS.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;

namespace LMS.Application.Features.Onboarding.Commands.Admin.AddOnboardingQuestion
{
    public class AddOnboardingQuestionCommandHandler
        : IRequestHandler<AddOnboardingQuestionCommand, Result<OnboardingQuestionDto>>
    {
        private readonly ILmsDbContext _context;
        private readonly IStringLocalizer<Messages> _localizer;

        public AddOnboardingQuestionCommandHandler(
            ILmsDbContext context,
            IStringLocalizer<Messages> localizer)
        {
            _context = context;
            _localizer = localizer;
        }

        public async Task<Result<OnboardingQuestionDto>> Handle(
            AddOnboardingQuestionCommand request,
            CancellationToken cancellationToken)
        {
            var question = new OnboardingQuestion
            {
                Key = request.Key.Trim(),
                Title = request.Title.Trim(),
                Type = request.Type,
                Order = request.Order,
                MinValue = request.MinValue,
                MaxValue = request.MaxValue,
                Step = request.Step
            };

            var order = 1;
            foreach (var optionInput in request.Options ?? Enumerable.Empty<OnboardingOptionInput>())
            {
                question.Options.Add(new OnboardingOption
                {
                    OnboardingQuestionId = question.Id,
                    Label = optionInput.Label.Trim(),
                    Value = optionInput.Value.Trim(),
                    Order = optionInput.Order != 0 ? optionInput.Order : order
                });
                order++;
            }

            await _context.OnboardingQuestions.AddAsync(question, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            var response = OnboardingMapper.ToDto(question);

            return Result<OnboardingQuestionDto>.Success(
                response,
                _localizer[LocalizationKeys.Onboarding.QuestionCreatedSuccess].Value,
                StatusCodes.Status201Created);
        }
    }
}
