using LMS.Application.Common.Exceptions;
using LMS.Application.Common.Interfaces;
using LMS.Application.Common.Models;
using LMS.Application.Features.Onboarding.DTOs;
using LMS.Application.Localization;
using LMS.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace LMS.Application.Features.Onboarding.Commands.Admin.AddOnboardingOption
{
    public class AddOnboardingOptionCommandHandler
        : IRequestHandler<AddOnboardingOptionCommand, Result<OnboardingOptionDto>>
    {
        private readonly ILmsDbContext _context;
        private readonly IStringLocalizer<Messages> _localizer;

        public AddOnboardingOptionCommandHandler(
            ILmsDbContext context,
            IStringLocalizer<Messages> localizer)
        {
            _context = context;
            _localizer = localizer;
        }

        public async Task<Result<OnboardingOptionDto>> Handle(
            AddOnboardingOptionCommand request,
            CancellationToken cancellationToken)
        {
            var question = await _context.OnboardingQuestions
                .Include(item => item.Options)
                .FirstOrDefaultAsync(item => item.Id == request.QuestionId, cancellationToken)
                ?? throw new NotFoundException(LocalizationKeys.Onboarding.QuestionNotFound);

            var option = new OnboardingOption
            {
                OnboardingQuestionId = question.Id,
                Label = request.Label.Trim(),
                Value = request.Value.Trim(),
                Order = request.Order != 0
                    ? request.Order
                    : question.Options.Count == 0 ? 1 : question.Options.Max(item => item.Order) + 1
            };

            await _context.OnboardingOptions.AddAsync(option, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            var response = new OnboardingOptionDto
            {
                Id = option.Id,
                Label = option.Label,
                Value = option.Value,
                Order = option.Order,
                IsActive = option.IsActive
            };

            return Result<OnboardingOptionDto>.Success(
                response,
                _localizer[LocalizationKeys.Onboarding.OptionCreatedSuccess].Value,
                StatusCodes.Status201Created);
        }
    }
}
