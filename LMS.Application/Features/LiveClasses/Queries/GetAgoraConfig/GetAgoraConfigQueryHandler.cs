using LMS.Application.Common.Interfaces;
using LMS.Application.Common.Models;
using LMS.Application.Features.LiveClasses.DTOs;
using LMS.Application.Localization;
using MediatR;
using Microsoft.Extensions.Localization;

namespace LMS.Application.Features.LiveClasses.Queries.GetAgoraConfig
{
    public class GetAgoraConfigQueryHandler : IRequestHandler<GetAgoraConfigQuery, Result<AgoraConfigDto>>
    {
        private readonly IAgoraTokenService _agoraTokenService;
        private readonly IStringLocalizer<Messages> _localizer;

        public GetAgoraConfigQueryHandler(
            IAgoraTokenService agoraTokenService,
            IStringLocalizer<Messages> localizer)
        {
            _agoraTokenService = agoraTokenService;
            _localizer = localizer;
        }

        public Task<Result<AgoraConfigDto>> Handle(GetAgoraConfigQuery request, CancellationToken cancellationToken)
        {
            var dto = new AgoraConfigDto
            {
                Enabled = _agoraTokenService.IsConfigured,
                AppId = _agoraTokenService.AppId,
                JoinLeadMinutes = _agoraTokenService.JoinLeadMinutes
            };

            return Task.FromResult(Result<AgoraConfigDto>.Success(dto, _localizer[LocalizationKeys.LiveClasses.ConfigRetrieved].Value));
        }
    }
}

