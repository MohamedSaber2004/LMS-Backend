using LMS.Domain.Common;

namespace LMS.Domain.Entities
{
    public class Country : BaseEntity<Guid>
    {
        public string CountryEnName { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;

        public virtual ICollection<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();
    }
}
