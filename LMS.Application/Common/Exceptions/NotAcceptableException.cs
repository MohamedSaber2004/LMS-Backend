using LMS.Application.Common.Exceptions.Base;
using LMS.Application.Localization;
using Microsoft.AspNetCore.Http;

namespace LMS.Application.Common.Exceptions
{
    public class NotAcceptableException : BaseCustomException
    {
        public NotAcceptableException()
            : base(LocalizationKeys.ExceptionMessages.NotAcceptable, StatusCodes.Status406NotAcceptable)
        {
        }

        public NotAcceptableException(string localizationKey, params object[] args)
            : base(localizationKey, StatusCodes.Status406NotAcceptable, args)
        {
        }
    }
}
