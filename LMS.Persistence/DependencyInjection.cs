using LMS.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LMS.Persistence
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("LMSConnectionString");
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                connectionString = configuration.GetConnectionString("DefaultConnection");
            }

            services.AddDbContext<LmsDbContext>(options =>
            {
                options.UseSqlServer(connectionString);
            });

            services.AddScoped<ILmsDbContext>(provider => provider.GetRequiredService<LmsDbContext>());

            return services;
        }
    }
}
