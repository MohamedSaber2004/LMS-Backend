using LMS.Application.Common.Exceptions.Base;
using LMS.Application.Localization;
using Microsoft.AspNetCore.Http;

namespace LMS.Application.Common.Exceptions
{
    public class GoneException : BaseCustomException
    {
        public GoneException()
            : base(LocalizationKeys.ExceptionMessages.Gone, StatusCodes.Status410Gone)
        {
        }

        public GoneException(string localizationKey, params object[] args)
            : base(localizationKey, StatusCodes.Status410Gone, args)
        {
        }
    }
}
