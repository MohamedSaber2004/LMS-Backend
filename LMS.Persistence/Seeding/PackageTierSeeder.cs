using LMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LMS.Persistence.Seeding
{
    public class PackageTierSeeder
    {
        public static async Task SeedPackageTiersAsync(LmsDbContext context)
        {
            foreach (var seed in GetTiers())
            {
                var alreadySeeded = await context.PackageTiers
                    .IgnoreQueryFilters()
                    .AnyAsync(tier => tier.TotalHours == seed.TotalHours);

                if (alreadySeeded)
                {
                    continue;
                }

                await context.PackageTiers.AddAsync(seed);
            }

            await context.SaveChangesAsync();
        }

        private static IReadOnlyList<PackageTier> GetTiers() => new List<PackageTier>
        {
            PackageTier.Create(
                "4 Academic Hours Package",
                4.0m,
                4,
                8,
                "Book up to 4 full sessions (50 min) or 8 half sessions (25 min), with complete scheduling flexibility.",
                1,
                20m,
                80m),
            PackageTier.Create(
                "8 Academic Hours Package",
                8.0m,
                8,
                16,
                "Book up to 8 full sessions (50 min) or 16 half sessions (25 min) with your chosen instructor.",
                2,
                18m,
                144m),
            PackageTier.Create(
                "12 Academic Hours Package",
                12.0m,
                12,
                24,
                "Book up to 12 full sessions (50 min) or 24 half sessions (25 min) for steady learning progress.",
                3,
                16m,
                192m),
            PackageTier.Create(
                "16 Academic Hours Package",
                16.0m,
                16,
                32,
                "Best value package for long-term commitment and comprehensive learning.",
                4,
                15m,
                240m)
        };
    }
}
