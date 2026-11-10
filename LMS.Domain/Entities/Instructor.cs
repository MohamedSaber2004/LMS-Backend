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
        public decimal CommissionRate { get; private set; } = 20.00m;
        public bool IsNativeSpeaker { get; private set; } = true;
        public int? Rank { get; private set; }
        public string Languages { get; private set; } = "Arabic";
        public decimal RatingAverage { get; private set; } = 0.00m;
        public int TotalReviews { get; private set; } = 0;
        public int CompletedSessionsCount { get; private set; } = 0;

        public bool VacationMode { get; private set; }

        public virtual InstructorApplication? Application { get; private set; }
        public virtual TeacherWallet? Wallet { get; private set; }

        public virtual ICollection<LearnerPackage> LearnerPackages { get; private set; } = new List<LearnerPackage>();
        public virtual ICollection<SessionReview> Reviews { get; private set; } = new List<SessionReview>();

        public virtual ICollection<InstructorCertificate> Certificates { get; private set; } = new List<InstructorCertificate>();

        public virtual ICollection<InstructorAvailabilitySlot> AvailabilitySlots { get; private set; } = new List<InstructorAvailabilitySlot>();

        public virtual ICollection<AvailabilityInstance> AvailabilityInstances { get; private set; } = new List<AvailabilityInstance>();

        public virtual ICollection<Session> Sessions { get; private set; } = new List<Session>();

        public virtual ICollection<CourseInstructor> CourseInstructors { get; private set; } = new List<CourseInstructor>();

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
            decimal? hourlyRate = null)
        {
            Specialization = specialization;
            Bio = bio;
            YearsOfExperience = yearsOfExperience;
            Education = education;
            LinkedInUrl = linkedInUrl;
            // Admin controls HourlyRate, teacher cannot overwrite it once set by admin unless not yet set
            if (hourlyRate.HasValue && HourlyRate <= 0)
            {
                HourlyRate = hourlyRate.Value;
            }
        }

        public IReadOnlyList<string> GetLanguagesList()
        {
            if (string.IsNullOrWhiteSpace(Languages))
            {
                return Array.Empty<string>();
            }

            return Languages
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();
        }

        public void SetAdminPricing(
            decimal hourlyRate,
            decimal commissionRate,
            bool? isNativeSpeaker = null,
            IEnumerable<string>? languages = null)
        {
            HourlyRate = hourlyRate;
            CommissionRate = commissionRate;
            if (isNativeSpeaker.HasValue) IsNativeSpeaker = isNativeSpeaker.Value;
            if (languages != null)
            {
                var cleaned = languages
                    .Where(l => !string.IsNullOrWhiteSpace(l))
                    .Select(l => l.Trim())
                    .Distinct(StringComparer.OrdinalIgnoreCase);

                Languages = string.Join(", ", cleaned);
            }
        }

        public void RecordSessionReview(int rating)
        {
            var clamped = Math.Clamp(rating, 1, 5);
            var currentSum = RatingAverage * TotalReviews;
            TotalReviews++;
            RatingAverage = Math.Round((currentSum + clamped) / TotalReviews, 2);
        }

        public void IncrementCompletedSessions()
        {
            CompletedSessionsCount++;
        }

        public void SetVacationMode(bool enabled)
        {
            VacationMode = enabled;
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
