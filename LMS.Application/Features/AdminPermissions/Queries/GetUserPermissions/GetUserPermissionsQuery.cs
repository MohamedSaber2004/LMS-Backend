using LMS.Application.Common.Exceptions;
using LMS.Application.Common.Interfaces;
using LMS.Application.Common.Models;
using LMS.Application.Common.Security;
using LMS.Application.Features.AdminPermissions.DTOs;
using LMS.Application.Localization;
using LMS.Domain.Entities;
using LMS.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace LMS.Application.Features.AdminPermissions.Queries.GetUserPermissions
{
    public class GetUserPermissionsQuery : IRequest<Result<UserPermissionsDto>>
    {
        public Guid UserId { get; set; }
    }

    public class GetUserPermissionsQueryHandler : IRequestHandler<GetUserPermissionsQuery, Result<UserPermissionsDto>>
    {
        private readonly ILmsDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IStringLocalizer<Messages> _localizer;

        public GetUserPermissionsQueryHandler(
            ILmsDbContext context,
            UserManager<ApplicationUser> userManager,
            IStringLocalizer<Messages> localizer)
        {
            _context = context;
            _userManager = userManager;
            _localizer = localizer;
        }

        public async Task<Result<UserPermissionsDto>> Handle(
            GetUserPermissionsQuery request,
            CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByIdAsync(request.UserId.ToString())
                ?? throw new NotFoundException(LocalizationKeys.Users.UserNotFound);

            var roles = await _userManager.GetRolesAsync(user);
            var isSuperAdmin = roles.Any(role => string.Equals(role, nameof(UserType.Admin), StringComparison.OrdinalIgnoreCase));

            var permissions = isSuperAdmin
                ? PermissionCatalog.All.Select(permission => (int)permission).OrderBy(code => code).ToList()
                : (await _context.AdminPermissionGrants
                    .AsNoTracking()
                    .Where(grant => grant.UserId == user.Id)
                    .Select(grant => (int)grant.Permission)
                    .ToListAsync(cancellationToken))
                    .OrderBy(code => code)
                    .ToList();

            var response = new UserPermissionsDto
            {
                UserId = user.Id,
                IsSuperAdmin = isSuperAdmin,
                Permissions = permissions
            };

            return Result<UserPermissionsDto>.Success(
                response,
                _localizer[LocalizationKeys.Permissions.GrantsRetrievedSuccess].Value);
        }
    }
}
