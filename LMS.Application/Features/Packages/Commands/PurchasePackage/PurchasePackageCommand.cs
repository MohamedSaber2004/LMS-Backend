using FluentValidation;
using LMS.API.Services;
using LMS.Application.Common.Exceptions;
using LMS.Application.Common.Helpers;
using LMS.Application.Common.Interfaces;
using LMS.Application.Common.Models;
using LMS.Application.Features.Packages.DTOs;
using LMS.Application.Localization;
using LMS.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace LMS.Application.Features.Packages.Commands.PurchasePackage
{
    public class PurchasePackageCommand : IRequest<Result<LearnerPackageDto>>
    {
        public Guid? InstructorId { get; set; }
        public Guid? TierId { get; set; }
        public decimal TotalHours { get; set; }
    }

    public class PurchasePackageCommandValidator : AbstractValidator<PurchasePackageCommand>
    {
        public PurchasePackageCommandValidator(
            ILmsDbContext context,
            IStringLocalizer<Messages> localizer)
        {
            RuleFor(x => x)
                .CustomAsync(async (command, validationContext, cancellationToken) =>
                {
                    if (command.TierId.HasValue && command.TierId.Value != Guid.Empty)
                    {
                        var tierExists = await context.PackageTiers
                            .AsNoTracking()
                            .AnyAsync(t => t.Id == command.TierId.Value, cancellationToken);

                        if (!tierExists)
                        {
                            validationContext.AddFailure(
                                nameof(command.TierId),
                                localizer[LocalizationKeys.Packages.TiersUnavailable].Value);
                        }
                        return;
                    }

                    if (command.TotalHours <= 0)
                    {
                        validationContext.AddFailure(
                            nameof(command.TotalHours),
                            localizer[LocalizationKeys.Packages.TierHoursMustBePositive].Value);
                        return;
                    }

                    var availableHours = await context.PackageTiers
                        .AsNoTracking()
                        .OrderBy(tier => tier.DisplayOrder)
                        .Select(tier => tier.TotalHours)
                        .ToListAsync(cancellationToken);

                    if (!availableHours.Contains(command.TotalHours))
                    {
                        validationContext.AddFailure(
                            nameof(command.TotalHours),
                            localizer[
                                LocalizationKeys.Packages.TierNotFound,
                                command.TotalHours.ToString("0.##"),
                                string.Join(", ", availableHours.Select(hour => hour.ToString("0.##")))].Value);
                    }
                });
        }
    }

    public class PurchasePackageCommandHandler : IRequestHandler<PurchasePackageCommand, Result<LearnerPackageDto>>
    {
        private readonly ILmsDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly IStringLocalizer<Messages> _localizer;

        public PurchasePackageCommandHandler(
            ILmsDbContext context,
            ICurrentUserService currentUserService,
            IStringLocalizer<Messages> localizer)
        {
            _context = context;
            _currentUserService = currentUserService;
            _localizer = localizer;
        }

        public async Task<Result<LearnerPackageDto>> Handle(
            PurchasePackageCommand request,
            CancellationToken cancellationToken)
        {
            var learnerId = await ActorOwnership.ResolveLearnerIdAsync(
                    _context, _currentUserService.UserId, cancellationToken)
                ?? throw new ForbiddenException(LocalizationKeys.ExceptionMessages.Forbidden);

            PackageTier? tier = null;
            if (request.TierId.HasValue && request.TierId.Value != Guid.Empty)
            {
                tier = await _context.PackageTiers
                    .AsNoTracking()
                    .FirstOrDefaultAsync(t => t.Id == request.TierId.Value, cancellationToken);
            }
            else if (request.TotalHours > 0)
            {
                tier = await _context.PackageTiers
                    .AsNoTracking()
                    .FirstOrDefaultAsync(t => t.TotalHours == request.TotalHours, cancellationToken);
            }

            if (tier == null)
            {
                throw new NotFoundException(LocalizationKeys.Packages.TiersUnavailable);
            }

            decimal hourlyRate = 0m;
            string? instructorFullName = null;

            if (request.InstructorId.HasValue && request.InstructorId.Value != Guid.Empty)
            {
                var instructor = await _context.Instructors
                    .Include(i => i.User)
                    .FirstOrDefaultAsync(i => i.Id == request.InstructorId.Value, cancellationToken);
                if (instructor != null)
                {
                    hourlyRate = instructor.HourlyRate;
                    instructorFullName = instructor.User?.FullName;
                }
            }

            var package = LearnerPackage.Create(
                learnerId,
                tier.Name,
                tier.TotalHours,
                hourlyRate,
                request.InstructorId);

            await _context.LearnerPackages.AddAsync(package, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            var dto = new LearnerPackageDto
            {
                Id = package.Id,
                InstructorId = package.InstructorId,
                InstructorFullName = instructorFullName ?? "All Instructors",
                PackageName = package.PackageName,
                TotalHours = package.TotalHours,
                RemainingHours = package.RemainingHours,
                HourlyRateSnapshot = package.HourlyRateSnapshot,
                TotalPrice = package.TotalPrice,
                IsActive = package.IsActive,
                CreatedAt = package.CreatedAt
            };

            return Result<LearnerPackageDto>.Success(
                dto,
                _localizer[LocalizationKeys.Packages.PurchaseSuccess].Value);
        }
    }
}

