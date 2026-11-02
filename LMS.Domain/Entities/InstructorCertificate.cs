using LMS.Domain.Common;
using LMS.Domain.Enums;

namespace LMS.Domain.Entities
{
    public class InstructorCertificate : BaseEntity<Guid>
    {
        public Guid InstructorId { get; private set; }
        public virtual Instructor Instructor { get; private set; } = null!;

        public string FilePath { get; private set; } = null!;
        public string OriginalFileName { get; private set; } = null!;
        public InstructorCertificateType Type { get; private set; }

        public static InstructorCertificate Create(
            Guid instructorId,
            string filePath,
            string originalFileName,
            InstructorCertificateType type)
        {
            return new InstructorCertificate
            {
                InstructorId = instructorId,
                FilePath = filePath,
                OriginalFileName = originalFileName,
                Type = type
            };
        }
    }
}
