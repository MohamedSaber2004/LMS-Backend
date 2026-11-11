namespace LMS.Domain.Enums
{
    public enum AdminPermission
    {
        UsersRead = 101,
        UsersAdd = 102,
        UsersUpdate = 103,
        UsersDelete = 104,

        PermissionsRead = 201,
        PermissionsUpdate = 203,

        CoursesRead = 301,
        CoursesAdd = 302,
        CoursesUpdate = 303,
        CoursesDelete = 304,

        HelpCenterRead = 401,
        HelpCenterAdd = 402,
        HelpCenterUpdate = 403,
        HelpCenterDelete = 404,

        OnboardingRead = 501,
        OnboardingAdd = 502,
        OnboardingUpdate = 503,
        OnboardingDelete = 504,

        PackagesRead = 601,
        PackagesUpdate = 603,

        PackageTiersRead = 701,
        PackageTiersAdd = 702,
        PackageTiersUpdate = 703,
        PackageTiersDelete = 704,

        SessionsRead = 801,
        SessionsUpdate = 803,

        TeachersRead = 901,
        TeachersUpdate = 903,

        InstructorApplicationsRead = 1001,
        InstructorApplicationsUpdate = 1003,

        InstructorPricingUpdate = 1103,

        InstructorAvailabilityRead = 1201,
        InstructorAvailabilityAdd = 1202,
        InstructorAvailabilityUpdate = 1203,
        InstructorAvailabilityDelete = 1204,

        CountriesRead = 1301,
        CountriesAdd = 1302,
        CountriesUpdate = 1303,
        CountriesDelete = 1304,

        FinancesRead = 1401
    }
}
