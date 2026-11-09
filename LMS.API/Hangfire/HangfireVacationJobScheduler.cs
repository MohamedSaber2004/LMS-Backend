using Hangfire;
using LMS.Application.Common.Interfaces;

namespace LMS.API.Hangfire
{
    public class HangfireVacationJobScheduler : IVacationJobScheduler
    {
        private readonly IBackgroundJobClient _backgroundJobClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<HangfireVacationJobScheduler> _logger;

        public HangfireVacationJobScheduler(
            IBackgroundJobClient backgroundJobClient,
            IConfiguration configuration,
            ILogger<HangfireVacationJobScheduler> logger)
        {
            _backgroundJobClient = backgroundJobClient;
            _configuration = configuration;
            _logger = logger;
        }

        public void EnqueueVacationConflictProcessing(Guid instructorId)
        {
            if (!_configuration.GetValue<bool>(HangfireExtensions.EnabledKey))
            {
                _logger.LogInformation("Hangfire is disabled. Skipping background job enqueue for instructor {InstructorId}.", instructorId);
                return;
            }

            _backgroundJobClient.Enqueue<IVacationBackgroundService>(
                service => service.ProcessVacationConflictsAsync(instructorId, CancellationToken.None));

            _logger.LogInformation("Enqueued Hangfire background job for vacation conflicts processing for instructor {InstructorId}.", instructorId);
        }
    }

    public class NoOpVacationJobScheduler : IVacationJobScheduler
    {
        public void EnqueueVacationConflictProcessing(Guid instructorId) { }
    }
}

