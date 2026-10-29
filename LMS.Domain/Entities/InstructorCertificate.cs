using LMS.Domain.Common;
using LMS.Domain.Enums;

namespace LMS.Domain.Entities
{
    public class InstructorCertificate : BaseEntity<Guid>
    {
        public Guid InstructorApplicationId { get; private set; }
        public virtual InstructorApplication InstructorApplication { get; private set; } = null!;

        public string FilePath { get; private set; } = null!;
        public string OriginalFileName { get; private set; } = null!;
        public InstructorCertificateType Type { get; private set; }

        public static InstructorCertificate Create(
            Guid instructorApplicationId,
            string filePath,
            string originalFileName,
            InstructorCertificateType type)
        {
            return new InstructorCertificate
            {
                InstructorApplicationId = instructorApplicationId,
                FilePath = filePath,
                OriginalFileName = originalFileName,
                Type = type
            };
        }
    }
}
