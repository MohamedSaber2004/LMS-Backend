using LMS.Domain.Enums;

namespace LMS.Application.Common.Interfaces
{
    public interface ICurrentLanguageService
    {
        Language Language { get; }
        string LanguageCode { get; }
    }
}
