namespace LMS.Application.Common.Interfaces
{
    public interface IVacationJobScheduler
    {
        void EnqueueVacationConflictProcessing(Guid instructorId);
    }
}

