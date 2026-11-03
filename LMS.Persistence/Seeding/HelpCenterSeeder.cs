using LMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LMS.Persistence.Seeding
{
    public class HelpCenterSeeder
    {
        public static async Task SeedHelpCenterAsync(LmsDbContext context)
        {
            foreach (var seed in GetCategories())
            {
                var alreadySeeded = await context.HelpCategories
                    .IgnoreQueryFilters()
                    .AnyAsync(category => category.Name == seed.Name);

                if (alreadySeeded)
                {
                    continue;
                }

                var category = HelpCategory.Create(seed.Name, seed.Order);
                await context.HelpCategories.AddAsync(category);

                foreach (var (question, answer, order) in seed.Articles)
                {
                    category.Articles.Add(HelpArticle.Create(category.Id, question, answer, order));
                }
            }

            await context.SaveChangesAsync();
        }

        private static IReadOnlyList<SeedCategory> GetCategories() => new List<SeedCategory>
        {
            new("Getting Started", 1, new[]
            {
                (
                    "How do I start learning on the platform?",
                    "Create a learner account and you are in straight away — learners do not need to confirm an email code.\n"
                    + "Once you are signed in, answer the short onboarding questions: your goal, your current level, what you want to focus on and your budget per lesson. "
                    + "The instructor list on the home screen is then filtered to the teachers who match what you asked for.",
                    1
                ),
                (
                    "What do the onboarding questions actually do?",
                    "They filter the instructor list for you — they are not a test and nothing is scored.\n"
                    + "You can change your answers later and the list updates. The budget you pick is compared with the hourly rate each instructor publishes.",
                    2
                ),
                (
                    "Does it cost anything to browse instructors?",
                    "No. Reading profiles, checking credentials and looking at a teacher's weekly availability is free.\n"
                    + "You only pay the hourly rate of the instructor you decide to work with.",
                    3
                ),
            }),
            new("Finding The Right Instructor", 2, new[]
            {
                (
                    "How do I choose the right instructor?",
                    "Compare teachers on three things: their specialization, the weekly windows they publish, and the hourly rate on their card.\n"
                    + "Open a profile to read their introduction, their education background and the certificates they have added. "
                    + "If two teachers look similar, pick the one whose availability matches the days you are actually free.",
                    1
                ),
                (
                    "Can I filter instructors by the days I am free?",
                    "Yes. Use the day-of-week filter, and add a time window if you only study in the evening or in the morning. "
                    + "Only instructors with a recurring window that overlaps your range are listed.",
                    2
                ),
                (
                    "What does an instructor's country have to do with my lessons?",
                    "It fixes the timezone. Availability windows are stored as the teacher's own local wall-clock times, "
                    + "so the app reads the country on their profile and converts those windows to your time before showing them.",
                    3
                ),
                (
                    "What does the hourly rate on an instructor card mean?",
                    "It is the price of one lesson hour in US dollars, set by the instructor when they registered. "
                    + "That is the amount shown to learners before they contact the teacher.",
                    4
                ),
            }),
            new("Lessons And Availability", 3, new[]
            {
                (
                    "How do I know when an instructor is free?",
                    "Every teacher has a weekly grid of repeating windows — a day of the week with a start and an end time. "
                    + "The grid is shown on their profile and is what the day and time filters search through.",
                    1
                ),
                (
                    "Why do the availability times not match my clock?",
                    "The times you see are the instructor's local hours. The conversion to your timezone happens in the app, "
                    + "using the country on that teacher's profile. If a window looks wrong, check the country first.",
                    2
                ),
                (
                    "What if an instructor has no availability listed yet?",
                    "It means no weekly window has been recorded for them, not that they are not teaching. "
                    + "Windows are added to the profile over time, so check again later or pick another teacher whose schedule already fits you.",
                    3
                ),
            }),
            new("For Instructors", 4, new[]
            {
                (
                    "How do I become an instructor?",
                    "Register with the instructor option and confirm the code sent to your email. "
                    + "Your application is then reviewed by an administrator. Once it is approved, your profile becomes visible to learners "
                    + "and your card shows up in their filtered list.",
                    1
                ),
                (
                    "What should I put in my profile?",
                    "Learners compare teachers on the specialization line, the bio, years of experience, the introduction video, "
                    + "your education background and any certificates you attach. A clear specialization and a short intro video make the "
                    + "difference on a list where several teachers match the same filters.",
                    2
                ),
                (
                    "How is my hourly rate used?",
                    "You set it in US dollars when you register, and it is shown on your card. "
                    + "Learners set a budget during onboarding, so the rate decides whether you appear in that list at all.",
                    3
                ),
                (
                    "How do weekly availability windows work?",
                    "A window is a day of the week plus a start and an end time, in your own local time, and it repeats every week. "
                    + "Two windows on the same day cannot overlap — the save is rejected if they do. Only the times you publish are searchable by learners.",
                    4
                ),
            }),
            new("Account And Security", 5, new[]
            {
                (
                    "How do I reset my password?",
                    "Use the forgot password option on the sign-in screen, follow the instructions sent to your email, then choose a new password. "
                    + "If the email does not arrive, check the spam folder before asking for it again.",
                    1
                ),
                (
                    "I did not receive the verification code.",
                    "Email verification codes are only sent to instructor registrations — learners are signed in straight away. "
                    + "Request a new code from the confirmation screen, and make sure the address you registered with is spelled correctly.",
                    2
                ),
                (
                    "How do I change my profile details?",
                    "Open your profile and edit it there: your name, your country, your preferred language and your profile picture are saved "
                    + "from the same form. The country matters for instructors — it is what the app uses to resolve their availability times.",
                    3
                ),
                (
                    "How do I delete my account?",
                    "Deleting your account is done from your profile settings. It signs every device out straight away and the account "
                    + "cannot be signed into again, so make sure that is what you want before you confirm.",
                    4
                ),
            }),
        };

        private sealed record SeedCategory(
            string Name,
            int Order,
            (string Question, string Answer, int Order)[] Articles);
    }
}
