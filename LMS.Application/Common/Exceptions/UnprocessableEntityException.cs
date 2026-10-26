using LMS.Application.Common.Exceptions.Base;
using LMS.Application.Localization;
using Microsoft.AspNetCore.Http;

namespace LMS.Application.Common.Exceptions
{
    public class UnprocessableEntityException : BaseCustomException
    {
        public UnprocessableEntityException()
            : base(LocalizationKeys.ExceptionMessages.UnprocessableEntity, StatusCodes.Status422UnprocessableEntity)
        {
        }

        public UnprocessableEntityException(string localizationKey, params object[] args)
            : base(localizationKey, StatusCodes.Status422UnprocessableEntity, args)
        {
        }
    }
}
