using LMS.Domain.Common;

namespace LMS.Domain.Entities
{
    public class CourseInstructor : BaseEntity<Guid>
    {
        public Guid CourseId { get; private set; }
        public virtual Course Course { get; private set; } = null!;

        public Guid InstructorId { get; private set; }
        public virtual Instructor Instructor { get; private set; } = null!;

        public static CourseInstructor Create(Guid courseId, Guid instructorId)
        {
            return new CourseInstructor
            {
                CourseId = courseId,
                InstructorId = instructorId
            };
        }
    }
}
