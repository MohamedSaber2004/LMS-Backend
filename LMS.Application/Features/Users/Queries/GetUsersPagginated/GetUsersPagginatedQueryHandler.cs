using LMS.Application.Common.Interfaces;
using LMS.Application.Common.Models;
using LMS.Application.Features.Users.DTOs;
using LMS.Application.Localization;
using LMS.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace LMS.Application.Features.Users.Queries.GetUsersPagginated
{
    public class GetUsersPagginatedQueryHandler
        : IRequestHandler<GetUsersPagginatedQuery, Result<PagginatedResult<UserDto>>>
    {
        private readonly ILmsDbContext _context;
        private readonly IStringLocalizer<Messages> _localizer;

        public GetUsersPagginatedQueryHandler(
            ILmsDbContext context,
            IStringLocalizer<Messages> localizer)
        {
            _context = context;
            _localizer = localizer;
        }

        public async Task<Result<PagginatedResult<UserDto>>> Handle(
            GetUsersPagginatedQuery request,
            CancellationToken cancellationToken)
        {
            var query = _context.ApplicationUsers
                .AsNoTracking()
                .Where(user => !user.IsDeleted);

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var search = request.Search.Trim();
                query = query.Where(user =>
                    user.FullName.Contains(search)
                    || (user.Email != null && user.Email.Contains(search))
                    || (user.PhoneNumber != null && user.PhoneNumber.Contains(search)));
            }

            if (request.IsActive.HasValue)
            {
                query = query.Where(user => user.IsActive == request.IsActive.Value);
            }

            if (request.Role.HasValue)
            {
                var roleName = request.Role.Value.ToString();
                query = query.Where(user => _context.UserRoles
                    .Any(userRole => userRole.UserId == user.Id
                        && _context.Roles.Any(role => role.Id == userRole.RoleId && role.Name == roleName)));
            }

            var totalCount = await query.CountAsync(cancellationToken);

            var pageNumber = request.PageNumber < 1
                ? PagginatedResult<UserDto>.DefaultPageNumber
                : request.PageNumber;
            var pageSize = request.PageSize < 1
                ? PagginatedResult<UserDto>.DefaultPageSize
                : Math.Min(request.PageSize, PagginatedResult<UserDto>.MaxPageSize);

            var users = await query
                .OrderBy(user => user.FullName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(user => new UserDto
                {
                    Id = user.Id,
                    FullName = user.FullName,
                    Email = user.Email ?? string.Empty,
                    PhoneNumber = user.PhoneNumber,
                    CountryId = user.CountryId,
                    CountryName = user.Country != null ? user.Country.CountryEnName : null,
                    IsActive = user.IsActive,
                    ProfilePictureUrl = user.ProfilePictureUrl,
                    CreatedAt = user.CreatedAt
                })
                .ToListAsync(cancellationToken);

            await PopulateRolesAsync(users, cancellationToken);

            var result = PagginatedResult<UserDto>.Create(
                users,
                totalCount,
                pageNumber,
                pageSize);

            return Result<PagginatedResult<UserDto>>.Success(
                result,
                _localizer[LocalizationKeys.Users.UsersRetrieved].Value);
        }

        private async Task PopulateRolesAsync(
            IReadOnlyCollection<UserDto> users,
            CancellationToken cancellationToken)
        {
            if (users.Count == 0)
            {
                return;
            }

            var userIds = users.Select(user => user.Id).ToList();

            var roles = await (from userRole in _context.UserRoles
                               join role in _context.Roles on userRole.RoleId equals role.Id
                               where userIds.Contains(userRole.UserId)
                               select new { userRole.UserId, role.Name })
                              .ToListAsync(cancellationToken);

            var roleLookup = roles
                .GroupBy(item => item.UserId)
                .ToDictionary(
                    group => group.Key,
                    group => group.Select(item => item.Name).FirstOrDefault());

            foreach (var user in users)
            {
                if (roleLookup.TryGetValue(user.Id, out var roleName)
                    && Enum.TryParse<UserType>(roleName, ignoreCase: true, out var role))
                {
                    user.Role = role;
                }
            }
        }
    }
}
