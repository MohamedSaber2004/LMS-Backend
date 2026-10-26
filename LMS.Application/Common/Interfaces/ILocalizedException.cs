namespace LMS.Application.Common.Interfaces
{
    public interface ILocalizedException
    {
        string LocalizationKey { get; }
        object[]? Args { get; }
        int StatusCode { get; }
    }
}
