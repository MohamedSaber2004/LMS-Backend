using LMS.Domain.Common;

namespace LMS.Domain.Entities
{
    public class PackageTier : BaseEntity<Guid>
    {
        public string Name { get; private set; } = null!;
        public decimal TotalHours { get; private set; }
        public int TotalFullSessions { get; private set; }
        public int TotalHalfSessions { get; private set; }
        public decimal PricePerHour { get; private set; }
        public decimal TotalPrice { get; private set; }
        public string Description { get; private set; } = null!;
        public int DisplayOrder { get; private set; }

        public static PackageTier Create(
            string name,
            decimal totalHours,
            int totalFullSessions,
            int totalHalfSessions,
            string description,
            int displayOrder,
            decimal pricePerHour = 0m,
            decimal? totalPrice = null)
        {
            var fullSessions = totalFullSessions > 0 ? totalFullSessions : (int)Math.Floor(totalHours);
            var halfSessions = totalHalfSessions > 0 ? totalHalfSessions : (int)Math.Floor(totalHours * 2);

            return new PackageTier
            {
                Name = name,
                TotalHours = totalHours,
                TotalFullSessions = fullSessions,
                TotalHalfSessions = halfSessions,
                Description = description,
                DisplayOrder = displayOrder,
                PricePerHour = pricePerHour,
                TotalPrice = totalPrice ?? Math.Round(totalHours * pricePerHour, 2)
            };
        }

        public void Update(
            string name,
            decimal totalHours,
            int totalFullSessions,
            int totalHalfSessions,
            string description,
            int displayOrder,
            decimal pricePerHour = 0m,
            decimal? totalPrice = null)
        {
            var fullSessions = totalFullSessions > 0 ? totalFullSessions : (int)Math.Floor(totalHours);
            var halfSessions = totalHalfSessions > 0 ? totalHalfSessions : (int)Math.Floor(totalHours * 2);

            Name = name;
            TotalHours = totalHours;
            TotalFullSessions = fullSessions;
            TotalHalfSessions = halfSessions;
            Description = description;
            DisplayOrder = displayOrder;
            PricePerHour = pricePerHour;
            TotalPrice = totalPrice ?? Math.Round(totalHours * pricePerHour, 2);
        }
    }
}
