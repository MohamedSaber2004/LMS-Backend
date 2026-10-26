using LMS.Application.Common.Exceptions.Base;
using LMS.Application.Localization;
using Microsoft.AspNetCore.Http;

namespace LMS.Application.Common.Exceptions
{
    public class TooManyRequestsException : BaseCustomException
    {
        public TooManyRequestsException()
            : base(LocalizationKeys.ExceptionMessages.TooManyRequests, StatusCodes.Status429TooManyRequests)
        {
        }

        public TooManyRequestsException(string localizationKey, params object[] args)
            : base(localizationKey, StatusCodes.Status429TooManyRequests, args)
        {
        }

        public TooManyRequestsException(string localizationKey, Exception innerException, params object[] args)
            : base(localizationKey, innerException, StatusCodes.Status429TooManyRequests, args)
        {
        }
    }
}
