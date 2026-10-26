using LMS.Application.Common.Exceptions.Base;
using LMS.Application.Localization;
using Microsoft.AspNetCore.Http;

namespace LMS.Application.Common.Exceptions
{
    public class PayloadTooLargeException : BaseCustomException
    {
        public PayloadTooLargeException()
            : base(LocalizationKeys.ExceptionMessages.PayloadTooLarge, StatusCodes.Status413PayloadTooLarge)
        {
        }

        public PayloadTooLargeException(string localizationKey, params object[] args)
            : base(localizationKey, StatusCodes.Status413PayloadTooLarge, args)
        {
        }
    }
}
