using LMS.Domain.Common;
using LMS.Domain.Enums;

namespace LMS.Domain.Entities
{
    public class AdminPermissionGrant : BaseEntity<Guid>
    {
        public Guid UserId { get; private set; }
        public AdminPermission Permission { get; private set; }

        public virtual ApplicationUser User { get; private set; } = null!;

        public static AdminPermissionGrant Create(Guid userId, AdminPermission permission) => new()
        {
            UserId = userId,
            Permission = permission
        };
    }
}
