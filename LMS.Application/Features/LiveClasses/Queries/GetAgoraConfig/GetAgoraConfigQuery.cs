using LMS.Application.Common.Models;
using LMS.Application.Features.LiveClasses.DTOs;
using MediatR;

namespace LMS.Application.Features.LiveClasses.Queries.GetAgoraConfig
{
    public class GetAgoraConfigQuery : IRequest<Result<AgoraConfigDto>>
    {
    }
}

