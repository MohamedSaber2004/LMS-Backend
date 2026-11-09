namespace LMS.Application.Common.Interfaces
{
    public interface IUserAccountDeletionService
    {
        Task HardDeleteUserAsync(Guid userId, CancellationToken cancellationToken = default);
    }
}

