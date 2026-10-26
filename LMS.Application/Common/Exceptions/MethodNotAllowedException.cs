using LMS.Application.Common.Exceptions.Base;
using LMS.Application.Localization;
using Microsoft.AspNetCore.Http;

namespace LMS.Application.Common.Exceptions
{
    public class MethodNotAllowedException : BaseCustomException
    {
        public MethodNotAllowedException()
            : base(LocalizationKeys.ExceptionMessages.MethodNotAllowed, StatusCodes.Status405MethodNotAllowed)
        {
        }

        public MethodNotAllowedException(string localizationKey, params object[] args)
            : base(localizationKey, StatusCodes.Status405MethodNotAllowed, args)
        {
        }
    }
}
