namespace LMS.Application.Common.Interfaces
{
    public interface IVacationBackgroundService
    {
        Task ProcessVacationConflictsAsync(Guid instructorId, CancellationToken cancellationToken = default);
    }
}

