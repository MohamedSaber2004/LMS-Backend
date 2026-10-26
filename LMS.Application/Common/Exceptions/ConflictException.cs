using LMS.Application.Common.Exceptions.Base;
using LMS.Application.Localization;
using Microsoft.AspNetCore.Http;

namespace LMS.Application.Common.Exceptions
{
    public class ConflictException : BaseCustomException
    {
        public ConflictException()
            : base(LocalizationKeys.ExceptionMessages.Conflict, StatusCodes.Status409Conflict)
        {
        }

        public ConflictException(string localizationKey, params object[] args)
            : base(localizationKey, StatusCodes.Status409Conflict, args)
        {
        }

        public ConflictException(string localizationKey, Exception innerException, params object[] args)
            : base(localizationKey, innerException, StatusCodes.Status409Conflict, args)
        {
        }
    }
}
