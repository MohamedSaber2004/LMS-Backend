namespace LMS.Application.Features.Packages.DTOs
{
    public class PackageTierDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal TotalHours { get; set; }
        public int TotalFullSessions { get; set; }
        public int TotalHalfSessions { get; set; }
        public string Description { get; set; } = string.Empty;
        public int DisplayOrder { get; set; }
    }
}
