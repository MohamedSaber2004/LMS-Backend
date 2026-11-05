using LMS.Domain.Entities;
using LMS.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace LMS.Persistence.Seeding
{
    public static class CoursesSeeder
    {
        private const string DemoLearnerEmail = "learner@gmail.com";
        private const string DraftActor = "courses-seeder";

        private const string SintelTrailer = "https://media.w3.org/2010/05/sintel/trailer.mp4";
        private const string BunnyMovie = "https://media.w3.org/2010/05/bunny/movie.mp4";
        private const string Movie300 = "https://media.w3.org/2010/05/video/movie_300.mp4";
        private const string FlowerClip = "https://mdn.github.io/shared-assets/videos/flower.mp4";
        private const string BunnyShortClip = "https://test-videos.co.uk/vids/bigbuckbunny/mp4/h264/360/Big_Buck_Bunny_360_10s_1MB.mp4";
        private const string SamplePdf = "https://www.w3.org/WAI/ER/tests/xhtml/testfiles/resources/pdf/dummy.pdf";

        public static async Task<SeedReport> SeedAsync(LmsDbContext context, CancellationToken cancellationToken)
        {
            var contentRows = 0;
            var seededCourses = new Dictionary<string, Course>();

            foreach (var seed in GetCatalog())
            {
                var existing = await context.Courses
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(course => course.Title == seed.Title, cancellationToken);

                if (existing is not null)
                {
                    seededCourses[seed.Title] = existing;
                    continue;
                }

                var course = Course.Create(seed.Title, seed.Description, seed.CoverUrl, seed.Level, seed.Order);
                await context.Courses.AddAsync(course, cancellationToken);
                contentRows += await context.SaveChangesAsync(cancellationToken);

                foreach (var lessonSeed in seed.Lessons)
                {
                    var lesson = Lesson.Create(
                        course.Id,
                        lessonSeed.Title,
                        lessonSeed.VideoUrl,
                        lessonSeed.StudyBookUrl,
                        lessonSeed.Order,
                        lessonSeed.IsFreeDemo);

                    foreach (var questionSeed in lessonSeed.Questions)
                    {
                        var question = QuizQuestion.Create(lesson.Id, questionSeed.Text, questionSeed.Order);
                        question.ReplaceOptions(questionSeed.Options.Select(option =>
                            QuizOption.Create(question.Id, option.Text, option.IsCorrect, option.Order)));

                        lesson.QuizQuestions.Add(question);
                    }

                    await context.Lessons.AddAsync(lesson, cancellationToken);
                }

                contentRows += await context.SaveChangesAsync(cancellationToken);

                if (!seed.IsPublished)
                {
                    course.SetActiveState(false, DraftActor);
                    contentRows += await context.SaveChangesAsync(cancellationToken);
                }

                seededCourses[seed.Title] = course;
            }

            var (learnerRows, demoLearnerFound) = await SeedLearnerActivityAsync(context, seededCourses, cancellationToken);

            return new SeedReport(contentRows, learnerRows, demoLearnerFound);
        }

        private static async Task<(int Rows, bool LearnerFound)> SeedLearnerActivityAsync(
            LmsDbContext context,
            Dictionary<string, Course> courses,
            CancellationToken cancellationToken)
        {
            var user = await context.Users
                .FirstOrDefaultAsync(item => item.Email == DemoLearnerEmail && !item.IsDeleted, cancellationToken);

            if (user is null)
            {
                return (0, false);
            }

            var learner = await context.Learners
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(item => item.UserId == user.Id && !item.IsDeleted, cancellationToken);

            if (learner is null)
            {
                return (0, false);
            }

            var written = 0;

            foreach (var title in new[]
            {
                "Egyptian Arabic 101: First Conversations",
                "Arabic for Work: Meetings and Emails"
            })
            {
                if (!courses.TryGetValue(title, out var course))
                {
                    continue;
                }

                if (await context.CourseEnrollments
                        .AnyAsync(enrollment => enrollment.CourseId == course.Id
                            && enrollment.LearnerId == learner.Id, cancellationToken))
                {
                    continue;
                }

                await context.CourseEnrollments
                    .AddAsync(CourseEnrollment.Create(course.Id, learner.Id), cancellationToken);
                written += await context.SaveChangesAsync(cancellationToken);
            }

            if (!courses.TryGetValue("Egyptian Arabic 101: First Conversations", out var firstCourse))
            {
                return (written, true);
            }

            var lessons = await context.Lessons
                .Where(lesson => lesson.CourseId == firstCourse.Id)
                .OrderBy(lesson => lesson.Order)
                .Take(2)
                .ToListAsync(cancellationToken);

            if (lessons.Count < 2)
            {
                return (written, true);
            }

            foreach (var (lesson, seconds, isCompleted) in new[]
            {
                (lessons[0], 52, true),
                (lessons[1], 18, false)
            })
            {
                if (await context.LessonProgress
                        .AnyAsync(progress => progress.LessonId == lesson.Id
                            && progress.LearnerId == learner.Id, cancellationToken))
                {
                    continue;
                }

                await context.LessonProgress
                    .AddAsync(LessonProgress.Create(lesson.Id, learner.Id, seconds, isCompleted), cancellationToken);
                written += await context.SaveChangesAsync(cancellationToken);
            }

            written += await SeedQuizAttemptAsync(context, lessons[0], learner.Id, cancellationToken);

            return (written, true);
        }

        private static async Task<int> SeedQuizAttemptAsync(
            LmsDbContext context,
            Lesson lesson,
            Guid learnerId,
            CancellationToken cancellationToken)
        {
            if (await context.QuizAttempts
                    .AnyAsync(attempt => attempt.LessonId == lesson.Id && attempt.LearnerId == learnerId, cancellationToken))
            {
                return 0;
            }

            var questions = await context.QuizQuestions
                .Include(question => question.Options)
                .Where(question => question.LessonId == lesson.Id && question.IsActive)
                .OrderBy(question => question.Order)
                .ToListAsync(cancellationToken);

            if (questions.Count == 0)
            {
                return 0;
            }

            var choices = new List<(QuizQuestion Question, QuizOption Option)>();
            var answered = 0;

            foreach (var question in questions)
            {
                var activeOptions = question.Options.Where(option => option.IsActive).OrderBy(option => option.Order).ToList();
                var correct = activeOptions.FirstOrDefault(option => option.IsCorrect);
                var wrong = activeOptions.FirstOrDefault(option => !option.IsCorrect);

                if (correct is null)
                {
                    continue;
                }

                choices.Add((question, answered < 2 || wrong is null ? correct : wrong));
                answered++;
            }

            if (choices.Count == 0)
            {
                return 0;
            }

            var attempt = QuizAttempt.Create(
                lesson.Id,
                learnerId,
                choices.Count(choice => choice.Option.IsCorrect),
                questions.Count);

            foreach (var (question, option) in choices)
            {
                attempt.AddAnswer(question.Id, option.Id);
            }

            await context.QuizAttempts.AddAsync(attempt, cancellationToken);
            return await context.SaveChangesAsync(cancellationToken);
        }

        private static IReadOnlyList<CourseSeed> GetCatalog() => new List<CourseSeed>
        {
            new(
                "Egyptian Arabic 101: First Conversations",
                "Hold your first real conversation in Egyptian Arabic. The course starts with greetings and "
                + "self-introduction, then moves through numbers and bargaining, ordering food and asking for directions. "
                + "Every lesson is a short clip you can watch once a day, and the first lesson is open without enrolling.",
                CourseLevel.Beginner,
                1,
                "/static/courses/egyptian-arabic-101.svg",
                true,
                new[]
                {
                    new LessonSeed(
                        "Greetings and Introducing Yourself",
                        SintelTrailer,
                        SamplePdf,
                        1,
                        true,
                        new[]
                        {
                            new QuestionSeed("Which greeting do you use when you meet someone in the afternoon?", 1, new[]
                            {
                                new OptionSeed("Marhaba", true, 1),
                                new OptionSeed("Ma'a salama", false, 2),
                                new OptionSeed("Tifaddal", false, 3),
                                new OptionSeed("Afwan", false, 4)
                            }),
                            new QuestionSeed("\"Ismi Ahmad. Esmak eih?\" is a way to…", 2, new[]
                            {
                                new OptionSeed("introduce yourself and ask the other person's name", true, 1),
                                new OptionSeed("ask for the price of something", false, 2),
                                new OptionSeed("apologise to a stranger", false, 3),
                                new OptionSeed("invite someone to eat", false, 4)
                            }),
                            new QuestionSeed("The reply to \"Kifak?\" is…", 3, new[]
                            {
                                new OptionSeed("Hamdulillah, antifak?", true, 1),
                                new OptionSeed("Shokran, ma'a salama", false, 2),
                                new OptionSeed("Ana za'lan", false, 3),
                                new OptionSeed("Tifaddal", false, 4)
                            })
                        }),
                    new LessonSeed(
                        "Numbers, Prices and Bargaining",
                        BunnyMovie,
                        null,
                        2,
                        false,
                        new[]
                        {
                            new QuestionSeed("What does \"thelatha\" mean?", 1, new[]
                            {
                                new OptionSeed("Three", true, 1),
                                new OptionSeed("Thirty", false, 2),
                                new OptionSeed("Thirteen", false, 3),
                                new OptionSeed("Third", false, 4)
                            }),
                            new QuestionSeed("\"Biazza al-hisab, min fadlak\" is said when you want to…", 2, new[]
                            {
                                new OptionSeed("ask for the bill", true, 1),
                                new OptionSeed("order a coffee", false, 2),
                                new OptionSeed("stop a taxi", false, 3),
                                new OptionSeed("thank the waiter", false, 4)
                            })
                        }),
                    new LessonSeed(
                        "Ordering Food and Drink",
                        Movie300,
                        null,
                        3,
                        false,
                        Array.Empty<QuestionSeed>()),
                    new LessonSeed(
                        "Asking for Directions",
                        FlowerClip,
                        null,
                        4,
                        false,
                        Array.Empty<QuestionSeed>())
                }),

            new(
                "Modern Standard Arabic Foundations",
                "Build the reading and writing base that dialects sit on: the alphabet with its vowel marks, the "
                + "root-and-pattern system behind almost every Arabic word, and short texts you read out loud. "
                + "Recommended before you try the work or travel courses.",
                CourseLevel.Elementary,
                2,
                "/static/courses/msa-foundations.svg",
                true,
                new[]
                {
                    new LessonSeed(
                        "The Alphabet and Vowel Marks",
                        BunnyShortClip,
                        null,
                        1,
                        true,
                        new[]
                        {
                            new QuestionSeed("How many letters are in the Arabic alphabet?", 1, new[]
                            {
                                new OptionSeed("28", true, 1),
                                new OptionSeed("26", false, 2),
                                new OptionSeed("30", false, 3),
                                new OptionSeed("24", false, 4)
                            }),
                            new QuestionSeed("The three short vowel marks are…", 2, new[]
                            {
                                new OptionSeed("fatha, kasra and damma", true, 1),
                                new OptionSeed("alif, waw and ya", false, 2),
                                new OptionSeed("shadda, madda and hamza", false, 3),
                                new OptionSeed("sukun, tanwin and wasla", false, 4)
                            })
                        }),
                    new LessonSeed(
                        "Roots and Patterns",
                        SintelTrailer,
                        SamplePdf,
                        2,
                        false,
                        new[]
                        {
                            new QuestionSeed("The root k-t-b produces words about…", 1, new[]
                            {
                                new OptionSeed("writing", true, 1),
                                new OptionSeed("opening", false, 2),
                                new OptionSeed("learning a language", false, 3),
                                new OptionSeed("travel", false, 4)
                            })
                        }),
                    new LessonSeed(
                        "Reading Short Texts",
                        BunnyMovie,
                        null,
                        3,
                        false,
                        Array.Empty<QuestionSeed>())
                }),

            new(
                "Arabic for Work: Meetings and Emails",
                "Professional Arabic for people who work with Arabic speakers. Open a meeting without sounding blunt, "
                + "write a follow-up email that lands correctly, and handle the small talk that decides whether a "
                + "deal moves forward.",
                CourseLevel.Intermediate,
                3,
                "/static/courses/arabic-for-work.svg",
                true,
                new[]
                {
                    new LessonSeed(
                        "Opening a Meeting Politely",
                        FlowerClip,
                        null,
                        1,
                        true,
                        new[]
                        {
                            new QuestionSeed("The usual opening greeting in a formal Arabic meeting is…", 1, new[]
                            {
                                new OptionSeed("as-salamu alaykum", true, 1),
                                new OptionSeed("ahlan wa sahlan bik", false, 2),
                                new OptionSeed("yawmak sa'id", false, 3),
                                new OptionSeed("ma'a salama", false, 4)
                            }),
                            new QuestionSeed("If a client arrives late, the face-saving move is to…", 2, new[]
                            {
                                new OptionSeed("welcome them without mentioning the delay", true, 1),
                                new OptionSeed("ask why they are late before starting", false, 2),
                                new OptionSeed("restart the meeting from the beginning", false, 3),
                                new OptionSeed("end the meeting early", false, 4)
                            })
                        }),
                    new LessonSeed(
                        "Writing a Follow-up Email",
                        Movie300,
                        SamplePdf,
                        2,
                        false,
                        new[]
                        {
                            new QuestionSeed("A follow-up email in Arabic should close with…", 1, new[]
                            {
                                new OptionSeed("a concrete next step and a thanks", true, 1),
                                new OptionSeed("only a greeting", false, 2),
                                new OptionSeed("an apology for writing", false, 3),
                                new OptionSeed("the full meeting notes", false, 4)
                            })
                        }),
                    new LessonSeed(
                        "Small Talk and Cultural Etiquette",
                        SintelTrailer,
                        null,
                        3,
                        false,
                        Array.Empty<QuestionSeed>())
                }),

            new(
                "Ramadan and Culture Vocabulary",
                "Vocabulary and manners for the Ramadan and Eid season: the words that come up at iftar, how to "
                + "congratulate someone on Eid, and the phrases guests hear most often. Draft while the lesson "
                + "recordings are replaced.",
                CourseLevel.Beginner,
                4,
                "/static/courses/ramadan-vocabulary.svg",
                false,
                new[]
                {
                    new LessonSeed(
                        "Words You Hear at Iftar",
                        BunnyMovie,
                        null,
                        1,
                        true,
                        Array.Empty<QuestionSeed>()),
                    new LessonSeed(
                        "Eid Greetings and Replies",
                        BunnyShortClip,
                        null,
                        2,
                        false,
                        Array.Empty<QuestionSeed>())
                })
        };

        private sealed record CourseSeed(
            string Title,
            string Description,
            CourseLevel Level,
            int Order,
            string? CoverUrl,
            bool IsPublished,
            IReadOnlyList<LessonSeed> Lessons);

        private sealed record LessonSeed(
            string Title,
            string VideoUrl,
            string? StudyBookUrl,
            int Order,
            bool IsFreeDemo,
            IReadOnlyList<QuestionSeed> Questions);

        private sealed record QuestionSeed(
            string Text,
            int Order,
            IReadOnlyList<OptionSeed> Options);

        private sealed record OptionSeed(string Text, bool IsCorrect, int Order);

        public sealed record SeedReport(int ContentRows, int LearnerRows, bool DemoLearnerFound)
        {
            public override string ToString()
            {
                return $"{ContentRows} content rows, {LearnerRows} learner rows"
                    + (DemoLearnerFound
                        ? string.Empty
                        : " (demo learner account not found — no learner activity seeded)");
            }
        }
    }
}
