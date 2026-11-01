using LMS.Domain.Entities;
using LMS.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace LMS.Persistence.Seeding
{
    public class OnboardingSeeder
    {
        private const string DemoLearnerEmail = "learner@gmail.com";

        public static async Task SeedOnboardingAsync(LmsDbContext context)
        {
            await SeedQuestionsAndOptionsAsync(context);
            await SeedDemoLearnerAnswersAsync(context);
        }

        private static async Task SeedQuestionsAndOptionsAsync(LmsDbContext context)
        {
            foreach (var question in GetQuestions())
            {
                if (await context.OnboardingQuestions.AnyAsync(item => item.Key == question.Key))
                {
                    continue;
                }

                await context.OnboardingQuestions.AddAsync(question);
            }

            await context.SaveChangesAsync();
        }

        private static async Task SeedDemoLearnerAnswersAsync(LmsDbContext context)
        {
            var learner = await context.Users
                .FirstOrDefaultAsync(user => user.Email == DemoLearnerEmail && !user.IsDeleted);

            if (learner is null)
            {
                return;
            }

            var questions = await context.OnboardingQuestions
                .Include(question => question.Options)
                .ToListAsync();

            if (questions.Count == 0)
            {
                return;
            }

            if (await context.OnboardingAnswers.AnyAsync(answer => answer.UserId == learner.Id))
            {
                return;
            }

            var answers = new List<OnboardingAnswer>();

            foreach (var (key, selection) in GetDemoAnswers())
            {
                var question = questions.FirstOrDefault(item => item.Key == key);
                if (question is null)
                {
                    continue;
                }

                if (question.Type == OnboardingQuestionType.Slider)
                {
                    answers.Add(new OnboardingAnswer
                    {
                        UserId = learner.Id,
                        OnboardingQuestionId = question.Id,
                        NumericValue = selection.NumericValue
                    });
                    continue;
                }

                foreach (var value in selection.OptionValues)
                {
                    var option = question.Options.FirstOrDefault(item => item.Value == value);
                    if (option is null)
                    {
                        continue;
                    }

                    answers.Add(new OnboardingAnswer
                    {
                        UserId = learner.Id,
                        OnboardingQuestionId = question.Id,
                        OnboardingOptionId = option.Id
                    });
                }
            }

            await context.OnboardingAnswers.AddRangeAsync(answers);
            await context.SaveChangesAsync();
        }

        private static IReadOnlyList<OnboardingQuestion> GetQuestions() => new List<OnboardingQuestion>
        {
            BuildQuestion("main_goal", "What is your main goal?", OnboardingQuestionType.SingleSelect, 1, new[]
            {
                ("Speak confidently", "speak_confidently"),
                ("Arabic for work", "arabic_for_work"),
                ("Connect with family", "connect_with_family"),
                ("Travel and culture", "travel_and_culture"),
                ("Academic Arabic", "academic_arabic"),
            }),
            BuildQuestion("arabic_level", "What is your current Arabic level?", OnboardingQuestionType.SingleSelect, 2, new[]
            {
                ("Complete beginner", "complete_beginner"),
                ("A few words", "a_few_words"),
                ("Basic conversations", "basic_conversations"),
                ("Intermediate", "intermediate"),
                ("Advanced", "advanced"),
            }),
            BuildQuestion("focus_areas", "What do you want to focus on?", OnboardingQuestionType.MultiSelect, 3, new[]
            {
                ("Speaking", "speaking"),
                ("Listening", "listening"),
                ("Pronunciation", "pronunciation"),
                ("Reading and writing", "reading_and_writing"),
                ("Grammar and vocabulary", "grammar_and_vocabulary"),
            }),
            BuildQuestion("which_arabic", "Which Arabic do you want to learn?", OnboardingQuestionType.SingleSelect, 4, new[]
            {
                ("Egyptian Arabic", "egyptian_arabic"),
                ("Modern Standard Arabic", "modern_standard_arabic"),
                ("Both / not sure", "both_or_not_sure"),
            }),
            BuildSlider("lesson_budget", "What is your budget per lesson (USD)?", 5, 5m, 100m, 5m),
        };

        /// <summary>Demo answers for the learner seeded by UserSeeder-style demo accounts.</summary>
        private static IReadOnlyList<(string Key, DemoAnswer Selection)> GetDemoAnswers() => new List<(string, DemoAnswer)>
        {
            ("main_goal", DemoAnswer.Options("speak_confidently")),
            ("arabic_level", DemoAnswer.Options("complete_beginner")),
            ("focus_areas", DemoAnswer.Options("speaking", "pronunciation")),
            ("which_arabic", DemoAnswer.Options("egyptian_arabic")),
            ("lesson_budget", DemoAnswer.Slider(25m)),
        };

        private static OnboardingQuestion BuildQuestion(
            string key,
            string title,
            OnboardingQuestionType type,
            int order,
            IReadOnlyList<(string Label, string Value)> options)
        {
            var question = new OnboardingQuestion
            {
                Key = key,
                Title = title,
                Type = type,
                Order = order,
            };

            var optionOrder = 1;
            foreach (var (label, value) in options)
            {
                question.Options.Add(new OnboardingOption
                {
                    OnboardingQuestionId = question.Id,
                    Label = label,
                    Value = value,
                    Order = optionOrder++,
                });
            }

            return question;
        }

        private static OnboardingQuestion BuildSlider(
            string key,
            string title,
            int order,
            decimal minValue,
            decimal maxValue,
            decimal step)
            => new()
            {
                Key = key,
                Title = title,
                Type = OnboardingQuestionType.Slider,
                Order = order,
                MinValue = minValue,
                MaxValue = maxValue,
                Step = step,
            };

        private sealed record DemoAnswer(string[] OptionValues, decimal? NumericValue)
        {
            public static DemoAnswer Options(params string[] values) => new(values, null);
            public static DemoAnswer Slider(decimal value) => new(Array.Empty<string>(), value);
        }
    }
}
