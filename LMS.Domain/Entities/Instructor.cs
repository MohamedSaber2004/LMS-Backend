using LMS.Domain.Common;

namespace LMS.Domain.Entities
{
    public class Instructor : BaseEntity<Guid>
    {
        public Guid UserId { get; private set; }
        public virtual ApplicationUser User { get; private set; } = null!;

        public string Specialization { get; private set; } = null!;
        public string Bio { get; private set; } = null!;
        public int YearsOfExperience { get; private set; }
        public string Education { get; private set; } = null!;
        public string? LinkedInUrl { get; private set; }
        public decimal HourlyRate { get; private set; }

        public virtual InstructorApplication? Application { get; private set; }

        public virtual ICollection<InstructorCertificate> Certificates { get; private set; } = new List<InstructorCertificate>();

        public virtual ICollection<InstructorAvailabilitySlot> AvailabilitySlots { get; private set; } = new List<InstructorAvailabilitySlot>();

        public static Instructor Create(
            Guid userId,
            string specialization,
            string bio,
            int yearsOfExperience,
            string education,
            string? linkedInUrl,
            decimal hourlyRate)
        {
            return new Instructor
            {
                UserId = userId,
                Specialization = specialization,
                Bio = bio,
                YearsOfExperience = yearsOfExperience,
                Education = education,
                LinkedInUrl = linkedInUrl,
                HourlyRate = hourlyRate
            };
        }

        public void UpdateProfile(
            string specialization,
            string bio,
            int yearsOfExperience,
            string education,
            string? linkedInUrl,
            decimal hourlyRate)
        {
            Specialization = specialization;
            Bio = bio;
            YearsOfExperience = yearsOfExperience;
            Education = education;
            LinkedInUrl = linkedInUrl;
            HourlyRate = hourlyRate;
        }

        public void AddCertificate(InstructorCertificate certificate)
        {
            Certificates.Add(certificate);
        }

        public void AddAvailabilitySlot(InstructorAvailabilitySlot slot)
        {
            AvailabilitySlots.Add(slot);
        }
    }
}
