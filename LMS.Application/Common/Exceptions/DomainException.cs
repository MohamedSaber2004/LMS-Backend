namespace LMS.Application.Common.Exceptions
{
    public class DomainException : Exception
    {
        public string LocalizationKey { get; }
        public object[]? Args { get; }

        public DomainException() : base()
        {
            LocalizationKey = string.Empty;
        }

        public DomainException(string message) : base(message)
        {
            LocalizationKey = message;
        }

        public DomainException(string message, string localizationKey, params object[] args) : base(message)
        {
            LocalizationKey = localizationKey;
            Args = args.Length > 0 ? args : null;
        }

        public DomainException(string message, Exception innerException) : base(message, innerException)
        {
            LocalizationKey = message;
        }

        public DomainException(string message, string localizationKey, Exception innerException, params object[] args) : base(message, innerException)
        {
            LocalizationKey = localizationKey;
            Args = args.Length > 0 ? args : null;
        }
    }
}
