using LMS.Application.Common.Exceptions;
using LMS.Application.Common.Interfaces;
using LMS.Application.Common.Models;
using LMS.Application.Features.Users.DTOs;
using LMS.Application.Localization;
using LMS.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace LMS.Application.Features.Users.Queries.GetUserById
{
    public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, Result<UserDto>>
    {
        private readonly ILmsDbContext _context;
        private readonly IStringLocalizer<Messages> _localizer;

        public GetUserByIdQueryHandler(ILmsDbContext context, IStringLocalizer<Messages> localizer)
        {
            _context = context;
            _localizer = localizer;
        }

        public async Task<Result<UserDto>> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
        {
            var user = await _context.ApplicationUsers
                .AsNoTracking()
                .Where(item => item.Id == request.Id && !item.IsDeleted)
                .Select(item => new UserDto
                {
                    Id = item.Id,
                    FullName = item.FullName,
                    Email = item.Email ?? string.Empty,
                    PhoneNumber = item.PhoneNumber,
                    CountryId = item.CountryId,
                    CountryName = item.Country != null ? item.Country.CountryEnName : null,
                    IsActive = item.IsActive,
                    ProfilePictureUrl = item.ProfilePictureUrl,
                    CreatedAt = item.CreatedAt
                })
                .FirstOrDefaultAsync(cancellationToken)
                ?? throw new NotFoundException(LocalizationKeys.Users.UserNotFound);

            var roleName = await (from userRole in _context.UserRoles
                                  join role in _context.Roles on userRole.RoleId equals role.Id
                                  where userRole.UserId == request.Id
                                  select role.Name)
                                 .FirstOrDefaultAsync(cancellationToken);

            if (Enum.TryParse<UserType>(roleName, ignoreCase: true, out var parsedRole))
            {
                user.Role = parsedRole;
            }

            return Result<UserDto>.Success(
                user,
                _localizer[LocalizationKeys.Users.UserRetrieved].Value);
        }
    }
}
