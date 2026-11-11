namespace LMS.Application.Features.AdminPermissions.DTOs
{
    public class PermissionOptionDto
    {
        public int Code { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
    }

    public class PermissionFeatureDto
    {
        public string Feature { get; set; } = string.Empty;
        public string FeatureName { get; set; } = string.Empty;
        public List<PermissionOptionDto> Permissions { get; set; } = new();
    }

    public class PermissionCatalogDto
    {
        public bool IsSuperAdmin { get; set; }
        public List<int> MyPermissions { get; set; } = new();
        public List<PermissionFeatureDto> Features { get; set; } = new();
    }

    public class UserPermissionsDto
    {
        public Guid UserId { get; set; }
        public bool IsSuperAdmin { get; set; }
        public List<int> Permissions { get; set; } = new();
    }
}
