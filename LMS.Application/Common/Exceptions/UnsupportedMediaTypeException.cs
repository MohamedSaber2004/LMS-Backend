using LMS.Application.Common.Exceptions.Base;
using LMS.Application.Localization;
using Microsoft.AspNetCore.Http;

namespace LMS.Application.Common.Exceptions
{
    public class UnsupportedMediaTypeException : BaseCustomException
    {
        public UnsupportedMediaTypeException()
            : base(LocalizationKeys.ExceptionMessages.UnsupportedMediaType, StatusCodes.Status415UnsupportedMediaType)
        {
        }

        public UnsupportedMediaTypeException(string localizationKey, params object[] args)
            : base(localizationKey, StatusCodes.Status415UnsupportedMediaType, args)
        {
        }
    }
}
