using LMS.Application.Localization;
using LMS.Domain.Enums;

namespace LMS.Application.Common.Security
{
    public static class PermissionCatalog
    {
        public const string ClaimType = "perm";
        public const string SubAdminRole = nameof(UserType.SubAdmin);

        public const string ReadAction = "Read";
        public const string AddAction = "Add";
        public const string UpdateAction = "Update";
        public const string DeleteAction = "Delete";

        public const int ReadSuffix = 1;
        public const int AddSuffix = 2;
        public const int UpdateSuffix = 3;
        public const int DeleteSuffix = 4;

        private static readonly AdminPermission[] AllValues = Enum.GetValues<AdminPermission>();

        public static IReadOnlyList<AdminPermission> All => AllValues;

        public static IReadOnlyList<string> Features { get; } = AllValues
            .Select(FeatureName)
            .Distinct()
            .ToArray();

        public static bool IsDefined(int permissionCode)
            => Enum.IsDefined(typeof(AdminPermission), permissionCode);

        public static string ActionName(AdminPermission permission)
        {
            var suffix = (int)permission % 100;
            return suffix switch
            {
                AddSuffix => AddAction,
                UpdateSuffix => UpdateAction,
                DeleteSuffix => DeleteAction,
                _ => ReadAction
            };
        }

        public static string FeatureName(AdminPermission permission)
        {
            var name = permission.ToString();
            var action = ActionName(permission);
            return name.EndsWith(action, StringComparison.Ordinal)
                ? name[..^action.Length]
                : name;
        }

        public static string FeatureDisplayNameKey(AdminPermission permission)
            => $"{LocalizationKeys.Permissions.FeaturePrefix}.{FeatureName(permission)}";

        public static string ActionDisplayNameKey(AdminPermission permission)
            => $"{LocalizationKeys.Permissions.ActionPrefix}.{ActionName(permission)}";

        public static IEnumerable<AdminPermission> ForFeature(string featureName)
            => AllValues.Where(permission => FeatureName(permission) == featureName);
    }
}
