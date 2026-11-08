using Hangfire;
using Hangfire.SqlServer;
using Serilog;

namespace LMS.API.Hangfire
{
    public static class HangfireExtensions
    {
        public const string EnabledKey = "Hangfire:Enabled";

        public static IServiceCollection AddSchedulingJobs(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            if (!configuration.GetValue<bool>(EnabledKey))
            {
                return services;
            }

            var connectionString = configuration.GetConnectionString("LMSConnectionString");
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                connectionString = configuration.GetConnectionString("DefaultConnection");
            }

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                Log.Warning(
                    "{ConfigKey} is true but no connection string is configured; scheduling jobs are disabled.",
                    EnabledKey);
                return services;
            }

            services.AddHangfire(config => config
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UseSqlServerStorage(connectionString, new SqlServerStorageOptions
                {
                    SchemaName = "HangFire",
                    PrepareSchemaIfNecessary = true,
                    CommandBatchMaxTimeout = TimeSpan.FromMinutes(5)
                }));

            services.AddHangfireServer(options =>
            {
                options.WorkerCount = 1;
                options.SchedulePollingInterval = TimeSpan.FromSeconds(30);
            });

            services.AddTransient<SchedulingJobs>();

            return services;
        }

        public static void RegisterSchedulingRecurringJobs(this WebApplication app)
        {
            if (!app.Configuration.GetValue<bool>(EnabledKey))
            {
                return;
            }

            app.UseHangfireDashboard("/hangfire");

            var utc = TimeZoneInfo.Utc;
            var options = new RecurringJobOptions { TimeZone = utc };

            RecurringJob.AddOrUpdate<SchedulingJobs>(
                "scheduling-materialize-availability",
                job => job.MaterializeAvailabilityAsync(),
                "0 2 * * *",
                options);

            RecurringJob.AddOrUpdate<SchedulingJobs>(
                "scheduling-expire-sessions",
                job => job.ExpireElapsedSessionsAsync(),
                "*/10 * * * *",
                options);

            RecurringJob.AddOrUpdate<SchedulingJobs>(
                "scheduling-cleanup-live-rooms",
                job => job.CleanupOrphanedLiveRoomsAsync(),
                "*/5 * * * *",
                options);
        }
    }
}
