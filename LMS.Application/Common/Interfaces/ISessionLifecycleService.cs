namespace LMS.Application.Common.Interfaces
{
    public interface ISessionLifecycleService
    {
        Task<int> ExpireElapsedSessionsAsync(CancellationToken cancellationToken = default);
    }
}
