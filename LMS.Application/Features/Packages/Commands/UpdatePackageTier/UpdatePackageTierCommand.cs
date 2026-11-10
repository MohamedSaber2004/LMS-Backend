using System.Text.Json.Serialization;
using FluentValidation;
using LMS.Application.Common.Exceptions;
using LMS.Application.Common.Interfaces;
using LMS.Application.Common.Models;
using LMS.Application.Features.Packages.DTOs;
using LMS.Application.Localization;
using LMS.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace LMS.Application.Features.Packages.Commands.UpdatePackageTier
{
    public class UpdatePackageTierCommand : IRequest<Result<PackageTierDto>>
    {
        [JsonIgnore]
        public Guid TierId { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal TotalHours { get; set; }
        public int TotalFullSessions { get; set; }
        public int TotalHalfSessions { get; set; }
        public string Description { get; set; } = string.Empty;
        public int DisplayOrder { get; set; }
    }

    public class UpdatePackageTierCommandValidator : AbstractValidator<UpdatePackageTierCommand>
    {
        private const int NameMaxLength = 150;
        private const int DescriptionMaxLength = 500;

        private readonly ILmsDbContext _context;
        private readonly IStringLocalizer<Messages> _localizer;

        public UpdatePackageTierCommandValidator(
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
                    if (command.TierId == Guid.Empty || command.TotalHours <= 0)
                    {
                        return;
                    }

                    var hours = command.TotalHours;
                    var isDuplicate = await _context.PackageTiers
                        .AnyAsync(t => t.Id != command.TierId && t.TotalHours == hours, cancellationToken);

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

    public class UpdatePackageTierCommandHandler : IRequestHandler<UpdatePackageTierCommand, Result<PackageTierDto>>
    {
        private readonly ILmsDbContext _context;
        private readonly IStringLocalizer<Messages> _localizer;

        public UpdatePackageTierCommandHandler(
            ILmsDbContext context,
            IStringLocalizer<Messages> localizer)
        {
            _context = context;
            _localizer = localizer;
        }

        public async Task<Result<PackageTierDto>> Handle(
            UpdatePackageTierCommand request,
            CancellationToken cancellationToken)
        {
            var tier = await _context.PackageTiers
                .FirstOrDefaultAsync(t => t.Id == request.TierId, cancellationToken)
                ?? throw new NotFoundException(nameof(PackageTier), request.TierId);

            var displayOrder = request.DisplayOrder == 0
                ? tier.DisplayOrder
                : request.DisplayOrder;

            tier.Update(
                request.Name.Trim(),
                request.TotalHours,
                request.TotalFullSessions,
                request.TotalHalfSessions,
                request.Description.Trim(),
                displayOrder);

            await _context.SaveChangesAsync(cancellationToken);

            var dto = PackageTierMapper.ToDto(tier);

            return Result<PackageTierDto>.Success(
                dto,
                _localizer[LocalizationKeys.Packages.TierUpdated].Value);
        }
    }
}
