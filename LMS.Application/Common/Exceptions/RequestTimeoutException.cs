using LMS.Application.Common.Exceptions.Base;
using LMS.Application.Localization;
using Microsoft.AspNetCore.Http;

namespace LMS.Application.Common.Exceptions
{
    public class RequestTimeoutException : BaseCustomException
    {
        public RequestTimeoutException()
            : base(LocalizationKeys.ExceptionMessages.RequestTimeout, StatusCodes.Status408RequestTimeout)
        {
        }

        public RequestTimeoutException(string localizationKey, params object[] args)
            : base(localizationKey, StatusCodes.Status408RequestTimeout, args)
        {
        }
    }
}
