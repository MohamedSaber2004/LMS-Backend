namespace LMS.Application.Features.Sessions
{
    public enum BookingBlocker
    {
        None = 0,
        InstructorNotBookable = 1,
        InstanceMissing = 2,
        SlotElapsed = 3,
        InstructorConflict = 4,
        InstructorBreak = 5,
        LearnerConflict = 6,
        LearnerBreak = 7
    }
}
