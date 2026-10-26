using LMS.Application.Common.Exceptions.Base;
using LMS.Application.Localization;
using Microsoft.AspNetCore.Http;

namespace LMS.Application.Common.Exceptions
{
    public class ForbiddenException : BaseCustomException
    {
        public ForbiddenException()
            : base(LocalizationKeys.ExceptionMessages.Forbidden, StatusCodes.Status403Forbidden)
        {
        }

        public ForbiddenException(string localizationKey, params object[] args)
            : base(localizationKey, StatusCodes.Status403Forbidden, args)
        {
        }

        public ForbiddenException(string localizationKey, Exception innerException, params object[] args)
            : base(localizationKey, innerException, StatusCodes.Status403Forbidden, args)
        {
        }
    }
}
