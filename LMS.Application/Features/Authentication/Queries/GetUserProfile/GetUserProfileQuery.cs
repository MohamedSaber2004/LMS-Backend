using LMS.Application.Common.Models;
using LMS.Application.Features.Authentication.DTOs;
using MediatR;

namespace LMS.Application.Features.Authentication.Queries.GetUserProfile
{
    public class GetUserProfileQuery : IRequest<Result<UserProfileDto>>
    {
    }
}
