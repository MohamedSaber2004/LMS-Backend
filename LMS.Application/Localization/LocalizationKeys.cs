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
            public const string EmailServiceNotConfigured = "ExceptionMessages.EmailServiceNotConfigured";
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
            public const string EducationRequired = "Authentication.EducationRequired";
            public const string EducationTooLong = "Authentication.EducationTooLong";
            public const string BioTooShort = "Authentication.BioTooShort";
            public const string InvalidIntroVideoFile = "Authentication.InvalidIntroVideoFile";
            public const string RegistrationCodeEmailSubject = "Authentication.RegistrationCodeEmailSubject";
            public const string RegistrationCodeEmailBody = "Authentication.RegistrationCodeEmailBody";
            public const string InstructorEmailVerifiedPending = "Authentication.InstructorEmailVerifiedPending";

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
            public const string HourlyRateRequired = "Authentication.HourlyRateRequired";
            public const string HourlyRateTooLarge = "Authentication.HourlyRateTooLarge";
            public const string InstructorApplicationNotResubmittable = "Authentication.InstructorApplicationNotResubmittable";
            public const string InstructorApplicationResubmitted = "Authentication.InstructorApplicationResubmitted";
            public const string MyInstructorApplicationRetrieved = "Authentication.MyInstructorApplicationRetrieved";
            public const string InstructorEmailNotVerified = "Authentication.InstructorEmailNotVerified";
        }

        public static class Onboarding
        {
            public const string QuestionKeyRequired = "Onboarding.QuestionKeyRequired";
            public const string QuestionKeyTooLong = "Onboarding.QuestionKeyTooLong";
            public const string QuestionKeyInvalidFormat = "Onboarding.QuestionKeyInvalidFormat";
            public const string QuestionKeyAlreadyExists = "Onboarding.QuestionKeyAlreadyExists";
            public const string TitleRequired = "Onboarding.TitleRequired";
            public const string TitleTooLong = "Onboarding.TitleTooLong";
            public const string InvalidQuestionType = "Onboarding.InvalidQuestionType";
            public const string SliderBoundsRequired = "Onboarding.SliderBoundsRequired";
            public const string InvalidSliderBounds = "Onboarding.InvalidSliderBounds";
            public const string OptionsRequired = "Onboarding.OptionsRequired";
            public const string SliderNoOptionsAllowed = "Onboarding.SliderNoOptionsAllowed";
            public const string OptionLabelRequired = "Onboarding.OptionLabelRequired";
            public const string OptionLabelTooLong = "Onboarding.OptionLabelTooLong";
            public const string OptionValueRequired = "Onboarding.OptionValueRequired";
            public const string OptionValueTooLong = "Onboarding.OptionValueTooLong";
            public const string DuplicateOptionValue = "Onboarding.DuplicateOptionValue";
            public const string QuestionNotFound = "Onboarding.QuestionNotFound";
            public const string QuestionNotActive = "Onboarding.QuestionNotActive";
            public const string OptionNotFound = "Onboarding.OptionNotFound";
            public const string AnswersRequired = "Onboarding.AnswersRequired";
            public const string DuplicateAnswerQuestion = "Onboarding.DuplicateAnswerQuestion";
            public const string AnswerOptionNotFromQuestion = "Onboarding.AnswerOptionNotFromQuestion";
            public const string SingleOptionRequired = "Onboarding.SingleOptionRequired";
            public const string AtLeastOneOptionRequired = "Onboarding.AtLeastOneOptionRequired";
            public const string NumericValueRequired = "Onboarding.NumericValueRequired";
            public const string NumericValueOutOfRange = "Onboarding.NumericValueOutOfRange";
            public const string QuestionsRetrieved = "Onboarding.QuestionsRetrieved";
            public const string QuestionCreatedSuccess = "Onboarding.QuestionCreatedSuccess";
            public const string QuestionUpdatedSuccess = "Onboarding.QuestionUpdatedSuccess";
            public const string QuestionDeletedSuccess = "Onboarding.QuestionDeletedSuccess";
            public const string OptionCreatedSuccess = "Onboarding.OptionCreatedSuccess";
            public const string OptionUpdatedSuccess = "Onboarding.OptionUpdatedSuccess";
            public const string OptionDeletedSuccess = "Onboarding.OptionDeletedSuccess";
            public const string AnswersSubmittedSuccess = "Onboarding.AnswersSubmittedSuccess";
            public const string AnswersRetrieved = "Onboarding.AnswersRetrieved";
            public const string InstructorsRetrieved = "Onboarding.InstructorsRetrieved";
            public const string InstructorDetailsRetrieved = "Onboarding.InstructorDetailsRetrieved";
            public const string PriceRangeInvalid = "Onboarding.PriceRangeInvalid";
            public const string PageNumberInvalid = "Onboarding.PageNumberInvalid";
            public const string PageSizeInvalid = "Onboarding.PageSizeInvalid";
            public const string SpecializationTooLong = "Onboarding.SpecializationTooLong";
        }

        public static class Availability
        {
            public const string InstructorIdRequired = "Availability.InstructorIdRequired";
            public const string SlotIdRequired = "Availability.SlotIdRequired";
            public const string SlotNotFound = "Availability.SlotNotFound";
            public const string InstructorNotFound = "Availability.InstructorNotFound";
            public const string InstructorNotApproved = "Availability.InstructorNotApproved";
            public const string DayInvalid = "Availability.DayInvalid";
            public const string DayRequiredForTimeFilter = "Availability.DayRequiredForTimeFilter";
            public const string FilterTimeRangeInvalid = "Availability.FilterTimeRangeInvalid";
            public const string EndTimeAfterStartTime = "Availability.EndTimeAfterStartTime";
            public const string OverlappingSlot = "Availability.OverlappingSlot";
            public const string SlotCreatedSuccess = "Availability.SlotCreatedSuccess";
            public const string SlotUpdatedSuccess = "Availability.SlotUpdatedSuccess";
            public const string SlotDeletedSuccess = "Availability.SlotDeletedSuccess";
            public const string SlotsRetrieved = "Availability.SlotsRetrieved";
            public const string AvailabilityRetrieved = "Availability.AvailabilityRetrieved";
            public const string PageNumberInvalid = "Availability.PageNumberInvalid";
            public const string PageSizeInvalid = "Availability.PageSizeInvalid";
            public const string SlotHasSessions = "Availability.SlotHasSessions";
            public const string TimezoneUnresolved = "Availability.TimezoneUnresolved";
            public const string DateInPast = "Availability.DateInPast";
            public const string NoWindowsOnDay = "Availability.NoWindowsOnDay";
            public const string DayUpdatedSuccess = "Availability.DayUpdatedSuccess";
            public const string WindowUpdatedSuccess = "Availability.WindowUpdatedSuccess";
            public const string VacationModeUpdatedSuccess = "Availability.VacationModeUpdatedSuccess";
            public const string DateRangeRequired = "Availability.DateRangeRequired";
            public const string DateRangeInvalid = "Availability.DateRangeInvalid";
            public const string DateRangeTooLarge = "Availability.DateRangeTooLarge";
            public const string CalendarRetrieved = "Availability.CalendarRetrieved";
            public const string DayHasSessions = "Availability.DayHasSessions";
            public const string VacationConflictsChecked = "Availability.VacationConflictsChecked";
        }

        public static class Sessions
        {
            public const string TabInvalid = "Sessions.TabInvalid";
            public const string PageNumberInvalid = "Sessions.PageNumberInvalid";
            public const string PageSizeInvalid = "Sessions.PageSizeInvalid";
            public const string SessionIdRequired = "Sessions.SessionIdRequired";
            public const string InstructorIdRequired = "Sessions.InstructorIdRequired";
            public const string SlotIdRequired = "Sessions.SlotIdRequired";
            public const string SlotNotFound = "Sessions.SlotNotFound";
            public const string SlotNotOpen = "Sessions.SlotNotOpen";
            public const string SlotElapsed = "Sessions.SlotElapsed";
            public const string DateRangeRequired = "Sessions.DateRangeRequired";
            public const string DateRangeInvalid = "Sessions.DateRangeInvalid";
            public const string DateRangeTooLarge = "Sessions.DateRangeTooLarge";
            public const string InstructorNotBookable = "Sessions.InstructorNotBookable";
            public const string InstructorBusy = "Sessions.InstructorBusy";
            public const string InstructorBreakTooShort = "Sessions.InstructorBreakTooShort";
            public const string LearnerBusy = "Sessions.LearnerBusy";
            public const string LearnerBreakTooShort = "Sessions.LearnerBreakTooShort";
            public const string ReasonTooLong = "Sessions.ReasonTooLong";
            public const string SessionNotFound = "Sessions.SessionNotFound";
            public const string SessionNotPending = "Sessions.SessionNotPending";
            public const string SessionNotCancellable = "Sessions.SessionNotCancellable";
            public const string SessionNotReschedulable = "Sessions.SessionNotReschedulable";
            public const string InstructorLockPassed = "Sessions.InstructorLockPassed";
            public const string LearnerLockPassed = "Sessions.LearnerLockPassed";
            public const string RescheduleIdRequired = "Sessions.RescheduleIdRequired";
            public const string RescheduleNotFound = "Sessions.RescheduleNotFound";
            public const string RescheduleNotPending = "Sessions.RescheduleNotPending";
            public const string RescheduleWindowInvalid = "Sessions.RescheduleWindowInvalid";
            public const string RescheduleAlreadyPending = "Sessions.RescheduleAlreadyPending";
            public const string RescheduleDurationChanged = "Sessions.RescheduleDurationChanged";
            public const string RescheduleConflicted = "Sessions.RescheduleConflicted";
            public const string RescheduleWithdrawnSuccess = "Sessions.RescheduleWithdrawnSuccess";
            public const string RescheduleNotWithdrawable = "Sessions.RescheduleNotWithdrawable";
            public const string RescheduleNotOwner = "Sessions.RescheduleNotOwner";
            public const string RescheduleRequestsRetrieved = "Sessions.RescheduleRequestsRetrieved";
            public const string ParticipantsRetrieved = "Sessions.ParticipantsRetrieved";
            public const string SlotsRetrieved = "Sessions.SlotsRetrieved";
            public const string SessionsRetrieved = "Sessions.SessionsRetrieved";
            public const string SessionRetrieved = "Sessions.SessionRetrieved";
            public const string BookingsRetrieved = "Sessions.BookingsRetrieved";
            public const string BookingCreatedSuccess = "Sessions.BookingCreatedSuccess";
            public const string SessionConfirmedSuccess = "Sessions.SessionConfirmedSuccess";
            public const string SessionDeclinedSuccess = "Sessions.SessionDeclinedSuccess";
            public const string SessionCancelledSuccess = "Sessions.SessionCancelledSuccess";
            public const string RescheduleRequestedSuccess = "Sessions.RescheduleRequestedSuccess";
            public const string RescheduleApprovedSuccess = "Sessions.RescheduleApprovedSuccess";
            public const string RescheduleDeclinedSuccess = "Sessions.RescheduleDeclinedSuccess";
            public const string DurationInvalid = "Sessions.DurationInvalid";
            public const string TrialAlreadyUsed = "Sessions.TrialAlreadyUsed";
            public const string InsufficientPackageHours = "Sessions.InsufficientPackageHours";
            public const string AttendanceAlreadyConfirmed = "Sessions.AttendanceAlreadyConfirmed";
            public const string CannotConfirmCancelled = "Sessions.CannotConfirmCancelled";
            public const string AttendanceConfirmedSuccess = "Sessions.AttendanceConfirmedSuccess";
        }

        public static class HelpCenter
        {
            public const string CategoryIdRequired = "HelpCenter.CategoryIdRequired";
            public const string CategoryNotFound = "HelpCenter.CategoryNotFound";
            public const string CategoryNameRequired = "HelpCenter.CategoryNameRequired";
            public const string CategoryNameTooLong = "HelpCenter.CategoryNameTooLong";
            public const string CategoryNameAlreadyExists = "HelpCenter.CategoryNameAlreadyExists";
            public const string CategoryInUse = "HelpCenter.CategoryInUse";
            public const string ArticleIdRequired = "HelpCenter.ArticleIdRequired";
            public const string ArticleNotFound = "HelpCenter.ArticleNotFound";
            public const string QuestionRequired = "HelpCenter.QuestionRequired";
            public const string QuestionTooLong = "HelpCenter.QuestionTooLong";
            public const string AnswerRequired = "HelpCenter.AnswerRequired";
            public const string AnswerTooLong = "HelpCenter.AnswerTooLong";
            public const string OrderMustBeNonNegative = "HelpCenter.OrderMustBeNonNegative";
            public const string SearchTooLong = "HelpCenter.SearchTooLong";
            public const string PageNumberInvalid = "HelpCenter.PageNumberInvalid";
            public const string PageSizeInvalid = "HelpCenter.PageSizeInvalid";
            public const string CategoryCreatedSuccess = "HelpCenter.CategoryCreatedSuccess";
            public const string CategoryUpdatedSuccess = "HelpCenter.CategoryUpdatedSuccess";
            public const string CategoryDeletedSuccess = "HelpCenter.CategoryDeletedSuccess";
            public const string CategoriesRetrieved = "HelpCenter.CategoriesRetrieved";
            public const string ArticleCreatedSuccess = "HelpCenter.ArticleCreatedSuccess";
            public const string ArticleUpdatedSuccess = "HelpCenter.ArticleUpdatedSuccess";
            public const string ArticleDeletedSuccess = "HelpCenter.ArticleDeletedSuccess";
            public const string ArticlesRetrieved = "HelpCenter.ArticlesRetrieved";
            public const string ArticleRetrieved = "HelpCenter.ArticleRetrieved";
        }

        public static class Courses
        {
            public const string TitleRequired = "Courses.TitleRequired";
            public const string TitleTooLong = "Courses.TitleTooLong";
            public const string OrderMustBeNonNegative = "Courses.OrderMustBeNonNegative";
            public const string PriceMustBePositive = "Courses.PriceMustBePositive";
            public const string PageNumberInvalid = "Courses.PageNumberInvalid";
            public const string PageSizeInvalid = "Courses.PageSizeInvalid";
            public const string SearchTooLong = "Courses.SearchTooLong";
            public const string CourseIdRequired = "Courses.CourseIdRequired";
            public const string CourseNotFound = "Courses.CourseNotFound";
            public const string DescriptionRequired = "Courses.DescriptionRequired";
            public const string DescriptionTooLong = "Courses.DescriptionTooLong";
            public const string LevelInvalid = "Courses.LevelInvalid";
            public const string CourseHasLessons = "Courses.CourseHasLessons";
            public const string CourseCreatedSuccess = "Courses.CourseCreatedSuccess";
            public const string CourseUpdatedSuccess = "Courses.CourseUpdatedSuccess";
            public const string CourseDeletedSuccess = "Courses.CourseDeletedSuccess";
            public const string CoursesRetrieved = "Courses.CoursesRetrieved";
            public const string CourseRetrieved = "Courses.CourseRetrieved";
            public const string InstructorIdRequired = "Courses.InstructorIdRequired";
            public const string DuplicateInstructors = "Courses.DuplicateInstructors";
            public const string InstructorNotAssignable = "Courses.InstructorNotAssignable";
            public const string InstructorsAssignedSuccess = "Courses.InstructorsAssignedSuccess";
            public const string InstructorsRetrieved = "Courses.InstructorsRetrieved";
            public const string LessonIdRequired = "Courses.LessonIdRequired";
            public const string LessonNotFound = "Courses.LessonNotFound";
            public const string VideoSourceRequired = "Courses.VideoSourceRequired";
            public const string UrlTooLong = "Courses.UrlTooLong";
            public const string FileTooLarge = "Courses.FileTooLarge";
            public const string InvalidVideoFile = "Courses.InvalidVideoFile";
            public const string InvalidStudyBookFile = "Courses.InvalidStudyBookFile";
            public const string InvalidCoverFile = "Courses.InvalidCoverFile";
            public const string LessonHasQuizQuestions = "Courses.LessonHasQuizQuestions";
            public const string LessonLocked = "Courses.LessonLocked";
            public const string LessonCreatedSuccess = "Courses.LessonCreatedSuccess";
            public const string LessonUpdatedSuccess = "Courses.LessonUpdatedSuccess";
            public const string LessonDeletedSuccess = "Courses.LessonDeletedSuccess";
            public const string LessonRetrieved = "Courses.LessonRetrieved";
            public const string QuestionIdRequired = "Courses.QuestionIdRequired";
            public const string QuestionNotFound = "Courses.QuestionNotFound";
            public const string QuestionTextRequired = "Courses.QuestionTextRequired";
            public const string QuestionTooLong = "Courses.QuestionTooLong";
            public const string OptionsRequired = "Courses.OptionsRequired";
            public const string OptionsCountInvalid = "Courses.OptionsCountInvalid";
            public const string OptionTextRequired = "Courses.OptionTextRequired";
            public const string OptionTextTooLong = "Courses.OptionTextTooLong";
            public const string ExactlyOneCorrectOptionRequired = "Courses.ExactlyOneCorrectOptionRequired";
            public const string QuestionCreatedSuccess = "Courses.QuestionCreatedSuccess";
            public const string QuestionUpdatedSuccess = "Courses.QuestionUpdatedSuccess";
            public const string QuestionDeletedSuccess = "Courses.QuestionDeletedSuccess";
            public const string AlreadyEnrolled = "Courses.AlreadyEnrolled";
            public const string EnrolledSuccess = "Courses.EnrolledSuccess";
            public const string MyCoursesRetrieved = "Courses.MyCoursesRetrieved";
            public const string WatchSecondsMustBeNonNegative = "Courses.WatchSecondsMustBeNonNegative";
            public const string ProgressUpdatedSuccess = "Courses.ProgressUpdatedSuccess";
            public const string AnswersRequired = "Courses.AnswersRequired";
            public const string AnswerQuestionInvalid = "Courses.AnswerQuestionInvalid";
            public const string AnswerOptionInvalid = "Courses.AnswerOptionInvalid";
            public const string DuplicateAnswerNotAllowed = "Courses.DuplicateAnswerNotAllowed";
            public const string QuizSubmittedSuccess = "Courses.QuizSubmittedSuccess";
            public const string LessonSessionScheduledSuccess = "Courses.LessonSessionScheduledSuccess";
            public const string LessonStartsAtRequired = "Courses.LessonStartsAtRequired";
            public const string LessonEndsAtRequired = "Courses.LessonEndsAtRequired";
            public const string LessonTimeRangeInvalid = "Courses.LessonTimeRangeInvalid";
            public const string LessonSessionWindowInPast = "Courses.LessonSessionWindowInPast";
        }

        public static class Countries
        {            public const string CountryEnNameRequired = "Countries.CountryEnNameRequired";
            public const string CountryEnNameTooLong = "Countries.CountryEnNameTooLong";
            public const string CodeRequired = "Countries.CodeRequired";
            public const string CodeTooLong = "Countries.CodeTooLong";
            public const string TimezoneIdTooLong = "Countries.TimezoneIdTooLong";
            public const string TimezoneIdInvalid = "Countries.TimezoneIdInvalid";
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

        public static class InstructorProfile
        {
            public const string ProfileRetrieved = "InstructorProfile.ProfileRetrieved";
            public const string ProfileUpdated = "InstructorProfile.ProfileUpdated";
            public const string EditNotAllowedBeforeApproval = "InstructorProfile.EditNotAllowedBeforeApproval";
        }

        public static class LiveClasses
        {
            public const string SessionIdRequired = "LiveClasses.SessionIdRequired";
            public const string ServiceNotConfigured = "LiveClasses.ServiceNotConfigured";
            public const string OnlyInstructorCanStart = "LiveClasses.OnlyInstructorCanStart";
            public const string OnlyInstructorCanEnd = "LiveClasses.OnlyInstructorCanEnd";
            public const string SessionMustBeConfirmed = "LiveClasses.SessionMustBeConfirmed";
            public const string JoinWindowNotOpen = "LiveClasses.JoinWindowNotOpen";
            public const string SessionWindowPassed = "LiveClasses.SessionWindowPassed";
            public const string ClassAlreadyEnded = "LiveClasses.ClassAlreadyEnded";
            public const string NotAuthorizedToJoin = "LiveClasses.NotAuthorizedToJoin";
            public const string NotAuthorizedToView = "LiveClasses.NotAuthorizedToView";
            public const string NotAuthorizedToRefreshToken = "LiveClasses.NotAuthorizedToRefreshToken";
            public const string RoomNotActive = "LiveClasses.RoomNotActive";
            public const string RoomNotFound = "LiveClasses.RoomNotFound";
            public const string CannotCancelWhileLive = "LiveClasses.CannotCancelWhileLive";
            public const string CannotRescheduleStartedOrEnded = "LiveClasses.CannotRescheduleStartedOrEnded";
            public const string ClassStartedSuccess = "LiveClasses.ClassStartedSuccess";
            public const string JoinedSuccess = "LiveClasses.JoinedSuccess";
            public const string LeftSuccess = "LiveClasses.LeftSuccess";
            public const string ClassEndedSuccess = "LiveClasses.ClassEndedSuccess";
            public const string TokenRefreshedSuccess = "LiveClasses.TokenRefreshedSuccess";
            public const string ConfigRetrieved = "LiveClasses.ConfigRetrieved";
            public const string SessionDetailsRetrieved = "LiveClasses.SessionDetailsRetrieved";
        }

        public static class Packages
        {
            public const string InstructorIdRequired = "Packages.InstructorIdRequired";
            public const string PurchaseSuccess = "Packages.PurchaseSuccess";
            public const string CatalogRetrieved = "Packages.CatalogRetrieved";
            public const string PackagesRetrieved = "Packages.PackagesRetrieved";
            public const string PackageHoursExceedTotal = "Packages.PackageHoursExceedTotal";
            public const string PackageAdjustmentHoursNegative = "Packages.PackageAdjustmentHoursNegative";
            public const string PackageAdjustmentReasonRequired = "Packages.PackageAdjustmentReasonRequired";
            public const string PackageAdjustmentReasonTooLong = "Packages.PackageAdjustmentReasonTooLong";
            public const string TierNotFound = "Packages.TierNotFound";
            public const string TiersUnavailable = "Packages.TiersUnavailable";
            public const string TiersRetrieved = "Packages.TiersRetrieved";
            public const string TierCreated = "Packages.TierCreated";
            public const string TierUpdated = "Packages.TierUpdated";
            public const string TierDeleted = "Packages.TierDeleted";
            public const string TierNameRequired = "Packages.TierNameRequired";
            public const string TierNameTooLong = "Packages.TierNameTooLong";
            public const string TierHoursMustBePositive = "Packages.TierHoursMustBePositive";
            public const string TierHoursAlreadyUsed = "Packages.TierHoursAlreadyUsed";
            public const string TierSessionsInvalid = "Packages.TierSessionsInvalid";
            public const string TierDescriptionRequired = "Packages.TierDescriptionRequired";
            public const string TierDescriptionTooLong = "Packages.TierDescriptionTooLong";
            public const string TierDisplayOrderInvalid = "Packages.TierDisplayOrderInvalid";
            public const string TierIdRequired = "Packages.TierIdRequired";
        }

        public static class Wallets
        {
            public const string SummaryRetrieved = "Wallets.SummaryRetrieved";
            public const string TransactionsRetrieved = "Wallets.TransactionsRetrieved";
            public const string NoTransactions = "Wallets.NoTransactions";
            public const string WithdrawalAmountMustBePositive = "Wallets.WithdrawalAmountMustBePositive";
            public const string WithdrawalAmountExceedsBalance = "Wallets.WithdrawalAmountExceedsBalance";
            public const string WithdrawalSuccess = "Wallets.WithdrawalSuccess";
        }

        public static class AdminManagement
        {
            public const string TeachersRetrievedSuccess = "AdminManagement.TeachersRetrievedSuccess";
            public const string TeacherDetailsRetrievedSuccess = "AdminManagement.TeacherDetailsRetrievedSuccess";
            public const string TeacherStatusUpdatedSuccess = "AdminManagement.TeacherStatusUpdatedSuccess";
            public const string PackagesRetrievedSuccess = "AdminManagement.PackagesRetrievedSuccess";
            public const string PackageAdjustedSuccess = "AdminManagement.PackageAdjustedSuccess";
            public const string SessionsRetrievedSuccess = "AdminManagement.SessionsRetrievedSuccess";
            public const string SessionCancelledSuccess = "AdminManagement.SessionCancelledSuccess";
            public const string OverviewRetrievedSuccess = "AdminManagement.OverviewRetrievedSuccess";
        }

        public static class TeacherManagement
        {
            public const string LearnersRetrievedSuccess = "TeacherManagement.LearnersRetrievedSuccess";
            public const string LearnerDetailsRetrievedSuccess = "TeacherManagement.LearnerDetailsRetrievedSuccess";
        }

        public static class Reviews
        {
            public const string SessionIdRequired = "Reviews.SessionIdRequired";
            public const string RatingRangeInvalid = "Reviews.RatingRangeInvalid";
            public const string CommentTooLong = "Reviews.CommentTooLong";
            public const string CannotReviewIncomplete = "Reviews.CannotReviewIncomplete";
            public const string AlreadyReviewed = "Reviews.AlreadyReviewed";
            public const string ReviewSuccess = "Reviews.ReviewSuccess";
        }

        public static class Pricing
        {
            public const string InstructorIdRequired = "Pricing.InstructorIdRequired";
            public const string HourlyRateMustBePositive = "Pricing.HourlyRateMustBePositive";
            public const string CommissionRateRange = "Pricing.CommissionRateRange";
            public const string PricingUpdatedSuccess = "Pricing.PricingUpdatedSuccess";
        }
    }
}
