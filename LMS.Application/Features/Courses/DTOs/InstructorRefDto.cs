namespace LMS.Application.Features.Courses.DTOs
{
    public class InstructorRefDto
    {
        public InstructorRefDto() { }

        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Specialization { get; set; } = string.Empty;
    }
}
