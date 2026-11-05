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
            public const string MyStatus = "my-status";
            public const string Resubmit = "resubmit";
        }

        public static class InstructorProfile
        {
            public const string Base = BaseRoutes.Base + "/instructor-profile";
        }

        public static class Onboarding
        {
            public const string Base = BaseRoutes.Base + "/onboarding";

            public const string Questions = "questions";
            public const string Answers = "answers";
            public const string MyAnswers = "my-answers";
            public const string Instructors = "instructors";

            public const string AdminQuestions = "admin/questions";
            public const string AdminQuestionById = "admin/questions/{id:guid}";
            public const string AdminQuestionOptions = "admin/questions/{id:guid}/options";
            public const string AdminOptionById = "admin/options/{id:guid}";
        }

        public static class InstructorAvailability
        {
            public const string Base = BaseRoutes.Base + "/instructor-availability";

            public const string GetAllPagginated = "pagginated";
            public const string Slots = "instructors/{instructorId:guid}/weekly-slots";
            public const string SlotById = "weekly-slots/{id:guid}";

            public const string My = "my";
            public const string MySlots = "my/weekly-slots";
            public const string MySlotById = "my/weekly-slots/{id:guid}";
            public const string MyVacationMode = "my/vacation-mode";
            public const string MyCalendar = "my/calendar";
            public const string MyCalendarDay = "my/calendar/day";
            public const string MyCalendarWindow = "my/calendar/window";
        }

        public static class Sessions
        {
            public const string Base = BaseRoutes.Base + "/sessions";

            public const string GetAllPagginated = "";
            public const string MyBookingsPagginated = "my-bookings";
            public const string BookableSlots = "bookable-slots";
            public const string GetById = "{id:guid}";
            public const string Confirm = "{id:guid}/confirm";
            public const string Decline = "{id:guid}/decline";
            public const string Cancel = "{id:guid}/cancel";
            public const string Reschedule = "{id:guid}/reschedule";
        }

        public static class RescheduleRequests
        {
            public const string Base = BaseRoutes.Base + "/reschedule-requests";

            public const string Approve = "{id:guid}/approve";
            public const string Decline = "{id:guid}/decline";
        }

        public static class HelpCenter
        {
            public const string Base = BaseRoutes.Base + "/help-center";

            public const string Categories = "categories";
            public const string Articles = "articles";
            public const string ArticleById = "articles/{id:guid}";

            public const string AdminCategories = "admin/categories";
            public const string AdminCategoryById = "admin/categories/{id:guid}";
            public const string AdminArticles = "admin/articles";
            public const string AdminArticleById = "admin/articles/{id:guid}";
        }

        public static class Courses
        {
            public const string Base = BaseRoutes.Base + "/courses";

            public const string Pagginated = "pagginated";
            public const string CourseById = "{id:guid}";
            public const string Enroll = "{id:guid}/enroll";
            public const string MyCourses = "my";
            public const string LessonContent = "lessons/{id:guid}/content";
            public const string LessonProgress = "lessons/{id:guid}/progress";
            public const string LessonQuiz = "lessons/{id:guid}/quiz";

            public const string AdminCourses = "admin/courses";
            public const string AdminCourseById = "admin/courses/{id:guid}";
            public const string AdminLessons = "admin/lessons";
            public const string AdminLessonById = "admin/lessons/{id:guid}";
            public const string AdminQuizQuestions = "admin/quiz-questions";
            public const string AdminQuizQuestionById = "admin/quiz-questions/{id:guid}";
            public const string AdminCourseInstructors = "admin/courses/{id:guid}/instructors";
            public const string AdminAssignableInstructors = "admin/instructors";
        }

        public static class Countries
        {            public const string Base = BaseRoutes.Base + "/countries";
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
