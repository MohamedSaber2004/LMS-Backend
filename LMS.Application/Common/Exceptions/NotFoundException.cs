using LMS.Application.Common.Exceptions.Base;
using LMS.Application.Localization;
using Microsoft.AspNetCore.Http;

namespace LMS.Application.Common.Exceptions
{
    public class NotFoundException : BaseCustomException
    {
        public NotFoundException()
            : base(LocalizationKeys.ExceptionMessages.NotFound, StatusCodes.Status404NotFound)
        {
        }

        public NotFoundException(string localizationKey, params object[] args)
            : base(localizationKey, StatusCodes.Status404NotFound, args)
        {
        }

        public NotFoundException(string localizationKey, Exception innerException, params object[] args)
            : base(localizationKey, innerException, StatusCodes.Status404NotFound, args)
        {
        }

        public NotFoundException(string entityName, object key)
            : base(LocalizationKeys.ExceptionMessages.NotFound, StatusCodes.Status404NotFound, entityName, key)
        {
        }
    }
}
