using LMS.Application.Common.Interfaces;

namespace LMS.API.Hangfire
{
    public class SchedulingJobs
    {
        private readonly IAvailabilityMaterializationService _materialization;
        private readonly ISessionLifecycleService _sessionLifecycle;
        private readonly ILogger<SchedulingJobs> _logger;

        public SchedulingJobs(
            IAvailabilityMaterializationService materialization,
            ISessionLifecycleService sessionLifecycle,
            ILogger<SchedulingJobs> logger)
        {
            _materialization = materialization;
            _sessionLifecycle = sessionLifecycle;
            _logger = logger;
        }

        public async Task MaterializeAvailabilityAsync()
        {
            var written = await _materialization.MaterializeAllAsync(CancellationToken.None);
        }

        public async Task ExpireElapsedSessionsAsync()
        {
            var closed = await _sessionLifecycle.ExpireElapsedSessionsAsync(CancellationToken.None);

            if (closed > 0)
            {
                _logger.LogInformation("Session lifecycle closed {Closed} elapsed session(s).", closed);
            }
        }
    }
}
