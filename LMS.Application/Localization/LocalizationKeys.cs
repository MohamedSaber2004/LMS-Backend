namespace LMS.Application.Localization
{
    public static class LocalizationKeys
    {
        public static class ExceptionMessages
        {
            public const string Validation = "ExceptionMessages.Validation";
            public const string InvalidModelState = "ExceptionMessages.InvalidModelState";
            public const string NotFound = "ExceptionMessages.NotFound";
            public const string BadRequest = "ExceptionMessages.BadRequest";
            public const string Unauthorized = "ExceptionMessages.Unauthorized";
            public const string Forbidden = "ExceptionMessages.Forbidden";
            public const string Conflict = "ExceptionMessages.Conflict";
            public const string InternalServerError = "ExceptionMessages.InternalServerError";
            public const string UnknownException = "ExceptionMessages.UnknownException";
            public const string TooManyRequests = "ExceptionMessages.TooManyRequests";
            public const string PayloadTooLarge = "ExceptionMessages.PayloadTooLarge";
            public const string UnprocessableEntity = "ExceptionMessages.UnprocessableEntity";
            public const string ServiceUnavailable = "ExceptionMessages.ServiceUnavailable";
            public const string NotAcceptable = "ExceptionMessages.NotAcceptable";
            public const string Gone = "ExceptionMessages.Gone";
            public const string MethodNotAllowed = "ExceptionMessages.MethodNotAllowed";
            public const string UnsupportedMediaType = "ExceptionMessages.UnsupportedMediaType";
            public const string RequestTimeout = "ExceptionMessages.RequestTimeout";
            public const string EmailAuthenticationFailed = "ExceptionMessages.EmailAuthenticationFailed";
            public const string EmailServerRejected = "ExceptionMessages.EmailServerRejected";
            public const string NetworkConnectionFailed = "ExceptionMessages.NetworkConnectionFailed";
            public const string GoogleAuthNotConfigured = "ExceptionMessages.GoogleAuthNotConfigured";
            public const string GoogleAuthValidationUnavailable = "ExceptionMessages.GoogleAuthValidationUnavailable";
            public const string TokenExpired = "ExceptionMessages.TokenExpired";
            public const string TokenRevoked = "ExceptionMessages.TokenRevoked";
            public const string TokenMissingClaims = "ExceptionMessages.TokenMissingClaims";
            public const string TokenInvalid = "ExceptionMessages.TokenInvalid";
        }

        public static class Attachments
        {
            public const string FileEmpty = "Attachments.FileEmpty";
            public const string FileNotFound = "Attachments.FileNotFound";
            public const string UploadFailed = "Attachments.UploadFailed";
            public const string InvalidFormat = "Attachments.InvalidFormat";
            public const string InvalidImageFormat = "Attachments.InvalidImageFormat";
            public const string InvalidVideoFormat = "Attachments.InvalidVideoFormat";
            public const string InvalidAudioFormat = "Attachments.InvalidAudioFormat";
            public const string InvalidFileFormat = "Attachments.InvalidFileFormat";
            public const string NoMediaProvided = "Attachments.NoMediaProvided";
            public const string ProtectedFile = "Attachments.ProtectedFile";
            public const string FileRequired = "Attachments.FileRequired";
            public const string StoredNameRequired = "Attachments.StoredNameRequired";
            public const string InvalidChunkParameters = "Attachments.InvalidChunkParameters";
            public const string UnsupportedMediaKind = "Attachments.UnsupportedMediaKind";
            public const string InvalidPlace = "Attachments.InvalidPlace";
        }

        public static class ActionResults
        {
            public const string Updated = "ActionResults.Updated";
            public const string Downloaded = "ActionResults.Downloaded";
        }

        public static class Authentication
        {
            public const string InvalidCredentials = "Authentication.InvalidCredentials";
            public const string AccountDeactivated = "Authentication.AccountDeactivated";
            public const string AccountLocked = "Authentication.AccountLocked";
            public const string EmailNotConfirmed = "Authentication.EmailNotConfirmed";
            public const string EmailRequired = "Authentication.EmailRequired";
            public const string InvalidEmailFormat = "Authentication.InvalidEmailFormat";
            public const string PasswordRequired = "Authentication.PasswordRequired";
            public const string LoginSuccess = "Authentication.LoginSuccess";
            public const string OtpSentSuccess = "Authentication.OtpSentSuccess";
            public const string OtpResentSuccess = "Authentication.OtpResentSuccess";
            public const string OtpRateLimited = "Authentication.OtpRateLimited";
            public const string OtpRequired = "Authentication.OtpRequired";
            public const string InvalidOtpFormat = "Authentication.InvalidOtpFormat";
            public const string OtpInvalidOrExpired = "Authentication.OtpInvalidOrExpired";
            public const string OtpVerifiedSuccess = "Authentication.OtpVerifiedSuccess";
            public const string UserNotFound = "Authentication.UserNotFound";
            public const string PasswordResetEmailSubject = "Authentication.PasswordResetEmailSubject";
            public const string PasswordResetEmailBody = "Authentication.PasswordResetEmailBody";
            public const string ResendOtpEmailSubject = "Authentication.ResendOtpEmailSubject";
            public const string ResendOtpEmailBody = "Authentication.ResendOtpEmailBody";
            public const string DefaultUserName = "Authentication.DefaultUserName";
            public const string OtpEmailGreeting = "Authentication.OtpEmailGreeting";
            public const string OtpEmailExpiryNotice = "Authentication.OtpEmailExpiryNotice";
            public const string OtpEmailFooter = "Authentication.OtpEmailFooter";
            public const string CurrentPasswordRequired = "Authentication.CurrentPasswordRequired";
            public const string CurrentPasswordIncorrect = "Authentication.CurrentPasswordIncorrect";
            public const string NewPasswordRequired = "Authentication.NewPasswordRequired";
            public const string NewPasswordMismatch = "Authentication.NewPasswordMismatch";
            public const string NewPasswordSameAsCurrent = "Authentication.NewPasswordSameAsCurrent";
            public const string PasswordTooShort = "Authentication.PasswordTooShort";
            public const string PasswordRequirementsNotMet = "Authentication.PasswordRequirementsNotMet";
            public const string ChangePasswordSuccess = "Authentication.ChangePasswordSuccess";
            public const string PasswordResetSuccess = "Authentication.PasswordResetSuccess";
            public const string RefreshTokenRequired = "Authentication.RefreshTokenRequired";
            public const string RefreshTokenSuccess = "Authentication.RefreshTokenSuccess";
            public const string LogoutSuccess = "Authentication.LogoutSuccess";
            public const string AccountDeletedSuccess = "Authentication.AccountDeletedSuccess";
            public const string FullNameRequired = "Authentication.FullNameRequired";
            public const string FullNameTooLong = "Authentication.FullNameTooLong";
            public const string ProfilePictureUrlTooLong = "Authentication.ProfilePictureUrlTooLong";
            public const string InvalidLanguage = "Authentication.InvalidLanguage";
            public const string CountryNotFound = "Authentication.CountryNotFound";
            public const string ProfileUpdatedSuccess = "Authentication.ProfileUpdatedSuccess";
            public const string ProfileRetrievedSuccess = "Authentication.ProfileRetrievedSuccess";

            public const string ConfirmPasswordRequired = "Authentication.ConfirmPasswordRequired";
            public const string PasswordsDoNotMatch = "Authentication.PasswordsDoNotMatch";
            public const string PhoneNumberRequired = "Authentication.PhoneNumberRequired";
            public const string InvalidPhoneNumber = "Authentication.InvalidPhoneNumber";
            public const string CountryRequired = "Authentication.CountryRequired";
            public const string InvalidRegistrationRole = "Authentication.InvalidRegistrationRole";
            public const string EmailAlreadyRegistered = "Authentication.EmailAlreadyRegistered";
            public const string RegistrationSuccess = "Authentication.RegistrationSuccess";
            public const string InstructorRegistrationPending = "Authentication.InstructorRegistrationPending";

            public const string SpecializationRequired = "Authentication.SpecializationRequired";
            public const string SpecializationTooLong = "Authentication.SpecializationTooLong";
            public const string BioRequired = "Authentication.BioRequired";
            public const string BioTooLong = "Authentication.BioTooLong";
            public const string YearsOfExperienceRequired = "Authentication.YearsOfExperienceRequired";
            public const string InvalidYearsOfExperience = "Authentication.InvalidYearsOfExperience";
            public const string LinkedInUrlTooLong = "Authentication.LinkedInUrlTooLong";
            public const string InvalidLinkedInUrl = "Authentication.InvalidLinkedInUrl";
            public const string CertificatesRequired = "Authentication.CertificatesRequired";
            public const string InvalidCertificateFile = "Authentication.InvalidCertificateFile";

            public const string ApplicationIdRequired = "Authentication.ApplicationIdRequired";
            public const string InstructorApplicationNotFound = "Authentication.InstructorApplicationNotFound";
            public const string InstructorApplicationAlreadyReviewed = "Authentication.InstructorApplicationAlreadyReviewed";
            public const string InstructorApprovedSuccess = "Authentication.InstructorApprovedSuccess";
            public const string InstructorRejectedSuccess = "Authentication.InstructorRejectedSuccess";
            public const string RejectionReasonRequired = "Authentication.RejectionReasonRequired";
            public const string RejectionReasonTooLong = "Authentication.RejectionReasonTooLong";
            public const string ReviewNotesTooLong = "Authentication.ReviewNotesTooLong";
            public const string InvalidApplicationStatus = "Authentication.InvalidApplicationStatus";
            public const string PageNumberInvalid = "Authentication.PageNumberInvalid";
            public const string PageSizeInvalid = "Authentication.PageSizeInvalid";
            public const string InstructorApplicationsRetrieved = "Authentication.InstructorApplicationsRetrieved";
        }

        public static class Countries
        {
            public const string CountryEnNameRequired = "Countries.CountryEnNameRequired";
            public const string CountryEnNameTooLong = "Countries.CountryEnNameTooLong";
            public const string CodeRequired = "Countries.CodeRequired";
            public const string CodeTooLong = "Countries.CodeTooLong";
            public const string CountryCodeAlreadyExists = "Countries.CountryCodeAlreadyExists";
            public const string CountryNameAlreadyExists = "Countries.CountryNameAlreadyExists";
            public const string CountryIdRequired = "Countries.CountryIdRequired";
            public const string CountryNotFound = "Countries.CountryNotFound";
            public const string CannotDeleteCountryInUse = "Countries.CannotDeleteCountryInUse";
            public const string CountryCreatedSuccess = "Countries.CountryCreatedSuccess";
            public const string CountryUpdatedSuccess = "Countries.CountryUpdatedSuccess";
            public const string CountryDeletedSuccess = "Countries.CountryDeletedSuccess";
            public const string CountryRetrieved = "Countries.CountryRetrieved";
            public const string CountriesRetrieved = "Countries.CountriesRetrieved";
            public const string PageNumberInvalid = "Countries.PageNumberInvalid";
            public const string PageSizeInvalid = "Countries.PageSizeInvalid";
        }

        public static class Users
        {
            public const string UserIdRequired = "Users.UserIdRequired";
            public const string UserNotFound = "Users.UserNotFound";
            public const string InvalidRole = "Users.InvalidRole";
            public const string CannotDeleteSelf = "Users.CannotDeleteSelf";
            public const string SearchTooLong = "Users.SearchTooLong";
            public const string NoFieldsToUpdate = "Users.NoFieldsToUpdate";
            public const string UserCreatedSuccess = "Users.UserCreatedSuccess";
            public const string UserUpdatedSuccess = "Users.UserUpdatedSuccess";
            public const string UserDeletedSuccess = "Users.UserDeletedSuccess";
            public const string UserRetrieved = "Users.UserRetrieved";
            public const string UsersRetrieved = "Users.UsersRetrieved";
            public const string PasswordChangedSuccess = "Users.PasswordChangedSuccess";
            public const string PageNumberInvalid = "Users.PageNumberInvalid";
            public const string PageSizeInvalid = "Users.PageSizeInvalid";
        }
    }
}
