namespace LMS.Application.Features.Countries.DTOs
{
    public class CountryDto
    {
        public Guid Id { get; set; }
        public string CountryEnName { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string? TimezoneId { get; set; }
        public bool IsActive { get; set; }
    }
}
