using System.Text.Json.Serialization;
using FluentValidation;
using LMS.API.Services;
using LMS.Application.Common.Exceptions;
using LMS.Application.Common.Interfaces;
using LMS.Application.Common.Models;
using LMS.Application.Localization;
using LMS.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace LMS.Application.Features.Packages.Commands.DeletePackageTier
{
    public class DeletePackageTierCommand : IRequest<Result<string>>
    {
        [JsonIgnore]
        public Guid TierId { get; set; }
    }

    public class DeletePackageTierCommandValidator : AbstractValidator<DeletePackageTierCommand>
    {
        public DeletePackageTierCommandValidator(IStringLocalizer<Messages> localizer)
        {
            RuleFor(x => x.TierId)
                .NotEmpty()
                .WithMessage(localizer[LocalizationKeys.Packages.TierIdRequired].Value);
        }
    }

    public class DeletePackageTierCommandHandler : IRequestHandler<DeletePackageTierCommand, Result<string>>
    {
        private readonly ILmsDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly IStringLocalizer<Messages> _localizer;

        public DeletePackageTierCommandHandler(
            ILmsDbContext context,
            ICurrentUserService currentUserService,
            IStringLocalizer<Messages> localizer)
        {
            _context = context;
            _currentUserService = currentUserService;
            _localizer = localizer;
        }

        public async Task<Result<string>> Handle(
            DeletePackageTierCommand request,
            CancellationToken cancellationToken)
        {
            var tier = await _context.PackageTiers
                .FirstOrDefaultAsync(t => t.Id == request.TierId, cancellationToken)
                ?? throw new NotFoundException(nameof(PackageTier), request.TierId);

            tier.MarkAsDeleted(_currentUserService.UserId.ToString());

            await _context.SaveChangesAsync(cancellationToken);

            var message = _localizer[LocalizationKeys.Packages.TierDeleted].Value;
            return Result<string>.Success(message, message);
        }
    }
}
