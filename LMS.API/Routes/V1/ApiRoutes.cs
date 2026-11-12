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
            public const string GetById = "{id:guid}";
            public const string Approve = "approve";
            public const string Reject = "reject";
            public const string SetPricing = "{id:guid}/pricing";
            public const string MyStatus = "my-status";
            public const string Resubmit = "resubmit";
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
            public const string Cancel = "{id:guid}/cancel";
            public const string ConfirmAttendance = "{id:guid}/confirm-attendance";
            public const string Reschedule = "{id:guid}/reschedule";
            public const string Participants = "{id:guid}/participants";
            public const string Pay = "{id:guid}/pay";
        }

        public static class RescheduleRequests
        {
            public const string Base = BaseRoutes.Base + "/reschedule-requests";

            public const string Approve = "{id:guid}/approve";
            public const string Decline = "{id:guid}/decline";
        }

        public static class Learner
        {
            public const string Base = BaseRoutes.Base + "/learner";

            public static class Teachers
            {
                public const string Base = Learner.Base + "/teachers";
                public const string GetAll = "";
                public const string GetById = "{id:guid}";
                public const string Schedule = "{id:guid}/schedule";
            }

            public static class Packages
            {
                public const string Base = Learner.Base + "/packages";
                public const string AvailablePlans = "available-plans";
                public const string Subscribe = "subscribe";
                public const string MyBalances = "my-balances";
                public const string Wallet = "wallet";
            }

            public static class Sessions
            {
                public const string Base = Learner.Base + "/sessions";
                public const string Rate = "{id:guid}/rate";
            }
        }

        public static class Admin
        {
            public const string Base = BaseRoutes.Base + "/admin";

            public static class Teachers
            {
                public const string Base = Admin.Base + "/teachers";
                public const string GetAll = "";
                public const string ToggleStatus = "{id:guid}/toggle-status";
                public const string Wallet = "{id:guid}/wallet";
            }

            public static class Sessions
            {
                public const string Base = Admin.Base + "/sessions";
                public const string GetAll = "";
                public const string GetById = "{id:guid}";
                public const string Cancel = "{id:guid}/cancel";
            }

            public static class Packages
            {
                public const string Base = Admin.Base + "/packages";
                public const string GetAll = "student-subscriptions";
                public const string StudentSubscriptions = "student-subscriptions";
                public const string Tiers = "tiers";
                public const string TierById = "tiers/{id:guid}";
            }

            public static class Finances
            {
                public const string Base = Admin.Base + "/finances";
                public const string Overview = "overview";
            }

            public static class Permissions
            {
                public const string Base = Admin.Base;
                public const string Catalog = "permissions";
                public const string ForUser = "users/{id:guid}/permissions";
            }
        }

        public static class Teacher
        {
            public const string Base = BaseRoutes.Base + "/teacher";

            public static class Students
            {
                public const string Base = Teacher.Base + "/students";
                public const string GetAll = "";
            }

            public static class Sessions
            {
                public const string Base = Teacher.Base + "/sessions";
                public const string Cancel = "{id:guid}/cancel";
                public const string RescheduleRequests = "{id:guid}/reschedule-requests";
                public const string Participants = "{id:guid}/participants";
            }

            public static class RescheduleRequests
            {
                public const string Base = Teacher.Base + "/reschedule-requests";
                public const string GetAll = "";
                public const string Withdraw = "{id:guid}/withdraw";
            }

            public static class Availability
            {
                public const string Base = Teacher.Base + "/availability";
                public const string GetWeeklySlots = "";
                public const string Slots = "weekly-slots";
                public const string SlotById = "weekly-slots/{id:guid}";
                public const string VacationMode = "vacation-mode";
                public const string Calendar = "calendar";
                public const string CalendarDay = "calendar/day";
                public const string CalendarWindow = "calendar/window";
            }

            public static class Wallet
            {
                public const string Base = Teacher.Base + "/wallet";
                public const string Summary = "";
                public const string Transactions = "transactions";
                public const string Withdraw = "withdraw";
            }

            public static class Profile
            {
                public const string Base = Teacher.Base + "/profile";
            }
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
            public const string AdminLessonSession = "admin/lessons/{id:guid}/session";
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

        public static class LiveClasses
        {
            public const string Base = BaseRoutes.Base + "/live-classes";

            public const string Config = "config";
            public const string Start = "{sessionId:guid}/start";
            public const string Join = "{sessionId:guid}/join";
            public const string Leave = "{sessionId:guid}/leave";
            public const string End = "{sessionId:guid}/end";
            public const string GetSession = "{sessionId:guid}";
            public const string Token = "{sessionId:guid}/token";
        }
    }
}
