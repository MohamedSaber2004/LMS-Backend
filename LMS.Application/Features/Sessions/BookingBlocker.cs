namespace LMS.Application.Features.Sessions
{
    public enum BookingBlocker
    {
        None = 0,
        InstructorConflict = 2,
        InstructorBreak = 3,
        LearnerConflict = 4,
        LearnerBreak = 5
    }
}
