namespace LMS.Application.Features.Packages.DTOs
{
    public class PackageCatalogItemDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal TotalHours { get; set; }
        public int TotalFullSessions { get; set; } // e.g. 4 sessions of 50 mins
        public int TotalHalfSessions { get; set; } // e.g. 8 sessions of 25 mins
        public decimal PricePerHour { get; set; }
        public decimal TotalPrice { get; set; }
        public string Description { get; set; } = string.Empty;
    }
}

