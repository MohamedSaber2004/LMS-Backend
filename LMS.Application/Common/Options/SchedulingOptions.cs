namespace LMS.Application.Common.Options
{
    public class SchedulingOptions
    {
        public const string SectionName = "Scheduling";

        public const int DefaultJoinTailMinutes = 30;

        public int DaysAhead { get; set; } = 30;

        public int JoinTailMinutes { get; set; } = DefaultJoinTailMinutes;

        public int MaxDaysAhead { get; set; } = 365;

        public int InstructorBatchSize { get; set; } = 50;

        public int MinimumBreakMinutes { get; set; } = 15;

        public int InstructorLockHours { get; set; } = 24;

        public int LearnerLockHours { get; set; } = 24;
    }
}
