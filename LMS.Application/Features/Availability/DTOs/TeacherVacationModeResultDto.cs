namespace LMS.Application.Features.Availability.DTOs
{
    public class TeacherVacationModeResultDto
    {
        public bool VacationModeEnabled { get; set; }
        public bool HasConflicts { get; set; }
        public int ConflictCount { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
