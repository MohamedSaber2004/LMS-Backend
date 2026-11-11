using LMS.API.Services;
using LMS.Application.Common.Helpers;
using LMS.Application.Common.Interfaces;
using LMS.Application.Common.Models;
using LMS.Application.Common.Security;
using LMS.Application.Features.AdminPermissions.DTOs;
using LMS.Application.Localization;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace LMS.Application.Features.AdminPermissions.Queries.GetPermissionCatalog
{
    public class GetPermissionCatalogQuery : IRequest<Result<PermissionCatalogDto>>
    {
    }

    public class GetPermissionCatalogQueryHandler : IRequestHandler<GetPermissionCatalogQuery, Result<PermissionCatalogDto>>
    {
        private readonly ILmsDbContext _context;
        private readonly ICurrentUserService _currentUser;
        private readonly IStringLocalizer<Messages> _localizer;

        public GetPermissionCatalogQueryHandler(
            ILmsDbContext context,
            ICurrentUserService currentUser,
            IStringLocalizer<Messages> localizer)
        {
            _context = context;
            _currentUser = currentUser;
            _localizer = localizer;
        }

        public async Task<Result<PermissionCatalogDto>> Handle(
            GetPermissionCatalogQuery request,
            CancellationToken cancellationToken)
        {
            var isSuperAdmin = ActorOwnership.IsAdmin(_currentUser);

            var granted = isSuperAdmin
                ? PermissionCatalog.All.Select(permission => (int)permission).ToList()
                : await _context.AdminPermissionGrants
                    .AsNoTracking()
                    .Where(grant => grant.UserId == _currentUser.UserId)
                    .Select(grant => (int)grant.Permission)
                    .ToListAsync(cancellationToken);

            var features = PermissionCatalog.Features
                .Select(featureName => new PermissionFeatureDto
                {
                    Feature = featureName,
                    FeatureName = _localizer[$"{LocalizationKeys.Permissions.FeaturePrefix}.{featureName}"].Value,
                    Permissions = PermissionCatalog.ForFeature(featureName)
                        .Select(permission => new PermissionOptionDto
                        {
                            Code = (int)permission,
                            Name = permission.ToString(),
                            Action = _localizer[PermissionCatalog.ActionDisplayNameKey(permission)].Value
                        })
                        .ToList()
                })
                .ToList();

            var response = new PermissionCatalogDto
            {
                IsSuperAdmin = isSuperAdmin,
                MyPermissions = granted,
                Features = features
            };

            return Result<PermissionCatalogDto>.Success(
                response,
                _localizer[LocalizationKeys.Permissions.CatalogRetrievedSuccess].Value);
        }
    }
}
