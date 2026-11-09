using LMS.Application.Common.Interfaces;
using LMS.Application.Common.Models;
using LMS.Application.Features.LiveClasses.DTOs;
using LMS.Application.Localization;
using MediatR;
using Microsoft.Extensions.Localization;

namespace LMS.Application.Features.LiveClasses.Queries.GetLiveClassConfig
{
    public class GetLiveClassConfigQueryHandler : IRequestHandler<GetLiveClassConfigQuery, Result<LiveClassConfigDto>>
    {
        private readonly ILiveClassTokenService _liveClassTokenService;
        private readonly IStringLocalizer<Messages> _localizer;

        public GetLiveClassConfigQueryHandler(
            ILiveClassTokenService liveClassTokenService,
            IStringLocalizer<Messages> localizer)
        {
            _liveClassTokenService = liveClassTokenService;
            _localizer = localizer;
        }

        public Task<Result<LiveClassConfigDto>> Handle(GetLiveClassConfigQuery request, CancellationToken cancellationToken)
        {
            var dto = new LiveClassConfigDto
            {
                Enabled = _liveClassTokenService.IsConfigured,
                Url = _liveClassTokenService.Url,
                JoinLeadMinutes = _liveClassTokenService.JoinLeadMinutes
            };

            return Task.FromResult(Result<LiveClassConfigDto>.Success(dto, _localizer[LocalizationKeys.LiveClasses.ConfigRetrieved].Value));
        }
    }
}
