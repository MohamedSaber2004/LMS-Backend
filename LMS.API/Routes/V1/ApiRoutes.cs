namespace LMS.API.Routes.V1
{
    public static class ApiRoutes
    {
        public static class Attachments
        {
            public const string Base = BaseRoutes.Base + "/attachments";

            public const string Upload = "upload";
            public const string UploadMultiple = "upload-multiple";
            public const string Download = "download";
            public const string Update = "update";
        }

        public static class Authentication
        {
            public const string Base = BaseRoutes.Base + "/authentication";

            public const string Login = "login";
            public const string Register = "register";
            public const string ForgotPassword = "forgot-password";
            public const string ResendOtp = "resend-otp";
            public const string VerifyOtp = "verify-otp";
            public const string ResetPassword = "reset-password";
            public const string ChangePassword = "change-password";
            public const string RefreshToken = "refresh-token";
            public const string Logout = "logout";
            public const string DeleteAccount = "delete-account";
            public const string UpdateUserInformation = "update-user-information";
            public const string GetUserProfile = "profile";
        }

        public static class InstructorApplications
        {
            public const string Base = BaseRoutes.Base + "/instructor-applications";

            public const string GetAll = "";
            public const string Approve = "approve";
            public const string Reject = "reject";
        }

        public static class Countries
        {
            public const string Base = BaseRoutes.Base + "/countries";

            public const string GetAllPagginated = "pagginated";
            public const string GetById = "{id:guid}";
            public const string Update = "update";
        }

        public static class Users
        {
            public const string Base = BaseRoutes.Base + "/users";

            public const string GetAllPagginated = "pagginated";
            public const string GetById = "{id:guid}";
            public const string Update = "update";
            public const string ChangePassword = "{id:guid}/change-password";
        }
    }
}
