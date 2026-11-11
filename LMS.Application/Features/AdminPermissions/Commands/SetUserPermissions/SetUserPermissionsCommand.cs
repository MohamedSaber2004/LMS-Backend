using FluentValidation;
using LMS.API.Services;
using LMS.Application.Common.Exceptions;
using LMS.Application.Common.Extensions;
using LMS.Application.Common.Helpers;
using LMS.Application.Common.Interfaces;
using LMS.Application.Common.Models;
using LMS.Application.Common.Security;
using LMS.Application.Features.AdminPermissions.DTOs;
using LMS.Application.Localization;
using LMS.Domain.Entities;
using LMS.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace LMS.Application.Features.AdminPermissions.Commands.SetUserPermissions
{
    public class SetUserPermissionsCommand : IRequest<Result<UserPermissionsDto>>
    {
        [System.Text.Json.Serialization.JsonIgnore]
        public Guid UserId { get; set; }

        public List<int> Permissions { get; set; } = new();
    }

    public class SetUserPermissionsCommandValidator : AbstractValidator<SetUserPermissionsCommand>
    {
        public SetUserPermissionsCommandValidator(
            ICurrentUserService currentUser,
            UserManager<ApplicationUser> userManager,
            IStringLocalizer<Messages> localizer)
        {
            RuleFor(command => command.UserId)
                .NotEmpty()
                .WithMessage(localizer[LocalizationKeys.Users.UserIdRequired].Value);

            RuleFor(command => command)
                .Custom((command, validationContext) =>
                {
                    var permissions = command.Permissions ?? new List<int>();

                    if (permissions.Count > PermissionCatalog.All.Count)
                    {
                        validationContext.AddFailure(
                            nameof(command.Permissions),
                            localizer[LocalizationKeys.Permissions.TooManyPermissionCodes, PermissionCatalog.All.Count].Value);
                    }

                    if (permissions.Distinct().Count() != permissions.Count)
                    {
                        var duplicate = permissions
                            .GroupBy(code => code)
                            .First(group => group.Count() > 1)
                            .Key;

                        validationContext.AddFailure(
                            nameof(command.Permissions),
                            localizer[LocalizationKeys.Permissions.DuplicatePermissionCode, duplicate].Value);
                    }

                    foreach (var code in permissions.Distinct())
                    {
                        if (PermissionCatalog.IsDefined(code))
                        {
                            continue;
                        }

                        validationContext.AddFailure(
                            nameof(command.Permissions),
                            localizer[LocalizationKeys.Permissions.UnknownPermissionCode, code].Value);
                    }

                    if (ActorOwnership.IsAdmin(currentUser))
                    {
                        return;
                    }

                    foreach (var code in permissions.Distinct())
                    {
                        if (currentUser.PermissionCodes.Contains(code))
                        {
                            continue;
                        }

                        validationContext.AddFailure(
                            nameof(command.Permissions),
                            localizer[LocalizationKeys.Permissions.CannotGrantPermissionYouDoNotHold, code].Value);
                    }
                });

            RuleFor(command => command)
                .CustomAsync(async (command, validationContext, cancellationToken) =>
                {
                    if (command.UserId == Guid.Empty)
                    {
                        return;
                    }

                    var user = await userManager.FindByIdAsync(command.UserId.ToString());
                    if (user is null)
                    {
                        return;
                    }

                    var roles = await userManager.GetRolesAsync(user);

                    var holdsAdminBoardRole = roles.Any(role =>
                        string.Equals(role, nameof(UserType.Admin), StringComparison.OrdinalIgnoreCase)
                        || string.Equals(role, PermissionCatalog.SubAdminRole, StringComparison.OrdinalIgnoreCase));

                    if (holdsAdminBoardRole)
                    {
                        return;
                    }

                    var currentRoles = roles.Count > 0
                        ? string.Join(", ", roles)
                        : "no role";

                    validationContext.AddFailure(
                        nameof(command.UserId),
                        localizer[LocalizationKeys.Permissions.GrantsRequireSubAdminRole, currentRoles].Value);
                });
        }
    }

    public class SetUserPermissionsCommandHandler : IRequestHandler<SetUserPermissionsCommand, Result<UserPermissionsDto>>
    {
        private readonly ILmsDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IStringLocalizer<Messages> _localizer;

        public SetUserPermissionsCommandHandler(
            ILmsDbContext context,
            UserManager<ApplicationUser> userManager,
            IStringLocalizer<Messages> localizer)
        {
            _context = context;
            _userManager = userManager;
            _localizer = localizer;
        }

        public async Task<Result<UserPermissionsDto>> Handle(
            SetUserPermissionsCommand request,
            CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByIdAsync(request.UserId.ToString())
                ?? throw new NotFoundException(LocalizationKeys.Users.UserNotFound);

            if (user.IsDeleted)
            {
                throw new NotFoundException(LocalizationKeys.Users.UserNotFound);
            }

            var requested = request.Permissions
                .Distinct()
                .Select(code => (AdminPermission)code)
                .ToList();

            var currentGrants = await _context.AdminPermissionGrants
                .Where(grant => grant.UserId == user.Id)
                .ToListAsync(cancellationToken);

            var revoked = currentGrants
                .Where(grant => !requested.Contains(grant.Permission))
                .ToList();

            var granted = requested
                .Where(permission => !currentGrants.Any(grant => grant.Permission == permission))
                .Select(permission => AdminPermissionGrant.Create(user.Id, permission))
                .ToList();

            if (revoked.Count > 0)
            {
                _context.AdminPermissionGrants.RemoveRange(revoked);
            }

            if (granted.Count > 0)
            {
                await _context.AdminPermissionGrants.AddRangeAsync(granted, cancellationToken);
            }

            if (revoked.Count > 0 || granted.Count > 0)
            {
                user.IncrementTokenVersion();

                var versionResult = await _userManager.UpdateAsync(user);
                if (!versionResult.Succeeded)
                {
                    return BuildIdentityFailure(versionResult);
                }

                await _context.RevokeAllRefreshTokensAsync(user.Id, cancellationToken);
            }

            await _context.SaveChangesAsync(cancellationToken);

            var response = new UserPermissionsDto
            {
                UserId = user.Id,
                IsSuperAdmin = (await _userManager.GetRolesAsync(user))
                    .Any(role => string.Equals(role, nameof(UserType.Admin), StringComparison.OrdinalIgnoreCase)),
                Permissions = requested
                    .Select(permission => (int)permission)
                    .OrderBy(code => code)
                    .ToList()
            };

            return Result<UserPermissionsDto>.Success(
                response,
                _localizer[LocalizationKeys.Permissions.GrantsUpdatedSuccess].Value);
        }

        private Result<UserPermissionsDto> BuildIdentityFailure(IdentityResult identityResult)
        {
            var errors = identityResult.Errors
                .Select(error => error.Description)
                .Where(description => !string.IsNullOrWhiteSpace(description))
                .ToArray();

            return Result<UserPermissionsDto>.Failure(
                _localizer[LocalizationKeys.ExceptionMessages.BadRequest].Value,
                StatusCodes.Status400BadRequest,
                errors);
        }
    }
}
