namespace LMS.Application.Common.Interfaces
{
    public interface IAvailabilityMaterializationService
    {
        Task<int> MaterializeAllAsync(CancellationToken cancellationToken = default);

        Task<int> MaterializeInstructorAsync(Guid instructorId, CancellationToken cancellationToken = default);
    }
}
