using LMS.Domain.Entities;

namespace LMS.Application.Features.Packages.DTOs
{
    public static class PackageTierMapper
    {
        public static PackageTierDto ToDto(PackageTier tier) => new()
        {
            Id = tier.Id,
            Name = tier.Name,
            TotalHours = tier.TotalHours,
            TotalFullSessions = tier.TotalFullSessions,
            TotalHalfSessions = tier.TotalHalfSessions,
            PricePerHour = tier.PricePerHour,
            TotalPrice = tier.TotalPrice,
            Description = tier.Description,
            DisplayOrder = tier.DisplayOrder
        };
    }
}
