using FluentValidation;
using LMS.Application.Common.Interfaces;
using LMS.Application.Common.Models;
using LMS.Application.Features.Packages.DTOs;
using LMS.Application.Localization;
using LMS.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace LMS.Application.Features.Packages.Commands.CreatePackageTier
{
    public class CreatePackageTierCommand : IRequest<Result<PackageTierDto>>
    {
        public string Name { get; set; } = string.Empty;
        public decimal TotalHours { get; set; }
        public int TotalFullSessions { get; set; }
        public int TotalHalfSessions { get; set; }
        public string Description { get; set; } = string.Empty;
        public int DisplayOrder { get; set; }
    }

    public class CreatePackageTierCommandValidator : AbstractValidator<CreatePackageTierCommand>
    {
        private const int NameMaxLength = 150;
        private const int DescriptionMaxLength = 500;

        private readonly ILmsDbContext _context;
        private readonly IStringLocalizer<Messages> _localizer;

        public CreatePackageTierCommandValidator(
            ILmsDbContext context,
            IStringLocalizer<Messages> localizer)
        {
            _context = context;
            _localizer = localizer;

            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage(localizer[LocalizationKeys.Packages.TierNameRequired].Value)
                .MaximumLength(NameMaxLength)
                .WithMessage(localizer[LocalizationKeys.Packages.TierNameTooLong, NameMaxLength].Value);

            RuleFor(x => x.TotalHours)
                .GreaterThan(0)
                .WithMessage(localizer[LocalizationKeys.Packages.TierHoursMustBePositive].Value);

            RuleFor(x => x.TotalFullSessions)
                .GreaterThanOrEqualTo(0)
                .WithMessage(localizer[LocalizationKeys.Packages.TierSessionsInvalid].Value);

            RuleFor(x => x.TotalHalfSessions)
                .GreaterThanOrEqualTo(0)
                .WithMessage(localizer[LocalizationKeys.Packages.TierSessionsInvalid].Value);

            RuleFor(x => x.Description)
                .NotEmpty()
                .WithMessage(localizer[LocalizationKeys.Packages.TierDescriptionRequired].Value)
                .MaximumLength(DescriptionMaxLength)
                .WithMessage(localizer[LocalizationKeys.Packages.TierDescriptionTooLong, DescriptionMaxLength].Value);

            RuleFor(x => x.DisplayOrder)
                .GreaterThanOrEqualTo(0)
                .WithMessage(localizer[LocalizationKeys.Packages.TierDisplayOrderInvalid].Value);

            RuleFor(x => x)
                .CustomAsync(async (command, validationContext, cancellationToken) =>
                {
                    if (command.TotalHours <= 0)
                    {
                        return;
                    }

                    var hours = command.TotalHours;
                    var isDuplicate = await _context.PackageTiers
                        .AnyAsync(t => t.TotalHours == hours, cancellationToken);

                    if (!isDuplicate)
                    {
                        return;
                    }

                    validationContext.AddFailure(
                        nameof(command.TotalHours),
                        _localizer[
                            LocalizationKeys.Packages.TierHoursAlreadyUsed,
                            hours.ToString("0.##")].Value);
                });
        }
    }

    public class CreatePackageTierCommandHandler : IRequestHandler<CreatePackageTierCommand, Result<PackageTierDto>>
    {
        private readonly ILmsDbContext _context;
        private readonly IStringLocalizer<Messages> _localizer;

        public CreatePackageTierCommandHandler(
            ILmsDbContext context,
            IStringLocalizer<Messages> localizer)
        {
            _context = context;
            _localizer = localizer;
        }

        public async Task<Result<PackageTierDto>> Handle(
            CreatePackageTierCommand request,
            CancellationToken cancellationToken)
        {
            var displayOrder = request.DisplayOrder;
            if (displayOrder == 0)
            {
                var maxOrder = await _context.PackageTiers
                    .AsNoTracking()
                    .Select(t => (int?)t.DisplayOrder)
                    .MaxAsync(cancellationToken);

                displayOrder = (maxOrder ?? 0) + 1;
            }

            var tier = PackageTier.Create(
                request.Name.Trim(),
                request.TotalHours,
                request.TotalFullSessions,
                request.TotalHalfSessions,
                request.Description.Trim(),
                displayOrder);

            await _context.PackageTiers.AddAsync(tier, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            var dto = PackageTierMapper.ToDto(tier);

            return Result<PackageTierDto>.Success(
                dto,
                _localizer[LocalizationKeys.Packages.TierCreated].Value,
                StatusCodes.Status201Created);
        }
    }
}
