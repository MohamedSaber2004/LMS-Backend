using LMS.Application.Common.Models;
using LMS.Application.Features.LiveClasses.DTOs;
using MediatR;

namespace LMS.Application.Features.LiveClasses.Queries.GetLiveClassConfig
{
    public class GetLiveClassConfigQuery : IRequest<Result<LiveClassConfigDto>>
    {
    }
}
