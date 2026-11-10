using LMS.Domain.Common;

namespace LMS.Domain.Entities
{
    public class PackageTier : BaseEntity<Guid>
    {
        public string Name { get; private set; } = null!;
        public decimal TotalHours { get; private set; }
        public int TotalFullSessions { get; private set; }
        public int TotalHalfSessions { get; private set; }
        public string Description { get; private set; } = null!;
        public int DisplayOrder { get; private set; }

        public static PackageTier Create(
            string name,
            decimal totalHours,
            int totalFullSessions,
            int totalHalfSessions,
            string description,
            int displayOrder)
        {
            return new PackageTier
            {
                Name = name,
                TotalHours = totalHours,
                TotalFullSessions = totalFullSessions,
                TotalHalfSessions = totalHalfSessions,
                Description = description,
                DisplayOrder = displayOrder
            };
        }

        public void Update(
            string name,
            decimal totalHours,
            int totalFullSessions,
            int totalHalfSessions,
            string description,
            int displayOrder)
        {
            Name = name;
            TotalHours = totalHours;
            TotalFullSessions = totalFullSessions;
            TotalHalfSessions = totalHalfSessions;
            Description = description;
            DisplayOrder = displayOrder;
        }
    }
}
