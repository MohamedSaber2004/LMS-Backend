using LMS.Application.Common.Exceptions.Base;
using LMS.Application.Localization;
using Microsoft.AspNetCore.Http;

namespace LMS.Application.Common.Exceptions
{
    public class ServiceUnavailableException : BaseCustomException
    {
        public ServiceUnavailableException()
            : base(LocalizationKeys.ExceptionMessages.ServiceUnavailable, StatusCodes.Status503ServiceUnavailable)
        {
        }

        public ServiceUnavailableException(string localizationKey, params object[] args)
            : base(localizationKey, StatusCodes.Status503ServiceUnavailable, args)
        {
        }
    }
}
