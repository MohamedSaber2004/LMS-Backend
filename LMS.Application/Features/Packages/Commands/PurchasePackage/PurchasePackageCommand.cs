using FluentValidation;
using FluentValidation.Validators;
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
        public Guid TierId { get; set; }
    }

    public class PurchasePackageCommandValidator : AbstractValidator<PurchasePackageCommand>
    {
        private const decimal ShortestLessonHours = 0.5m;

        private readonly ILmsDbContext _context;
        private readonly ICurrentUserService _currentUser;
        private readonly IStringLocalizer<Messages> _localizer;

        public PurchasePackageCommandValidator(
            ILmsDbContext context,
            ICurrentUserService currentUser,
            IStringLocalizer<Messages> localizer)
        {
            _context = context;
            _currentUser = currentUser;
            _localizer = localizer;

            RuleFor(x => x.TierId)
                .NotEmpty()
                .WithMessage(localizer[LocalizationKeys.Packages.TiersUnavailable].Value)
                .MustAsync(async (tierId, cancellationToken) =>
                {
                    return await context.PackageTiers
                        .AsNoTracking()
                        .AnyAsync(t => t.Id == tierId, cancellationToken);
                })
                .WithMessage(localizer[LocalizationKeys.Packages.TiersUnavailable].Value);

            RuleFor(command => command)
                .CustomAsync(ValidateAsync);
        }

        private async Task ValidateAsync(
            PurchasePackageCommand command,
            CustomContext context,
            CancellationToken cancellationToken)
        {
            var learnerId = await ActorOwnership.ResolveLearnerIdAsync(
                    _context, _currentUser.UserId, cancellationToken)
                ?? throw new ForbiddenException(
                    LocalizationKeys.ExceptionMessages.LearnerProfileMissing, _currentUser.UserId);

            var planWithCredit = await _context.LearnerPackages
                .Where(lp => lp.LearnerId == learnerId
                    && lp.IsActive
                    && lp.RemainingHours >= ShortestLessonHours)
                .OrderBy(lp => lp.CreatedAt)
                .Select(lp => new { lp.PackageName, lp.RemainingHours })
                .FirstOrDefaultAsync(cancellationToken);

            if (planWithCredit is not null)
            {
                context.AddFailure(
                    nameof(PurchasePackageCommand.TierId),
                    _localizer[
                        LocalizationKeys.Packages.ActivePlanCreditRemaining,
                        planWithCredit.RemainingHours,
                        planWithCredit.PackageName].Value);
            }
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
                ?? throw new ForbiddenException(
                    LocalizationKeys.ExceptionMessages.LearnerProfileMissing, _currentUserService.UserId);

            var tier = await _context.PackageTiers
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == request.TierId, cancellationToken)
                ?? throw new NotFoundException(LocalizationKeys.Packages.TiersUnavailable);

            var package = LearnerPackage.Create(
                learnerId,
                tier.Name,
                tier.TotalHours,
                tier.PricePerHour,
                instructorId: null,
                totalPrice: tier.TotalPrice);

            await _context.LearnerPackages.AddAsync(package, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            var dto = new LearnerPackageDto
            {
                Id = package.Id,
                InstructorId = null,
                InstructorFullName = _localizer[LocalizationKeys.Packages.AllInstructors].Value,
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

