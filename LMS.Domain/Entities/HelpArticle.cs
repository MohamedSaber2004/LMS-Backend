using LMS.Domain.Common;

namespace LMS.Domain.Entities
{
    public class HelpArticle : BaseEntity<Guid>
    {
        public Guid CategoryId { get; private set; }
        public virtual HelpCategory Category { get; private set; } = null!;

        public string Question { get; private set; } = string.Empty;

        public string Answer { get; private set; } = string.Empty;

        public int Order { get; private set; }

        public static HelpArticle Create(Guid categoryId, string question, string answer, int order)
        {
            return new HelpArticle
            {
                CategoryId = categoryId,
                Question = question,
                Answer = answer,
                Order = order
            };
        }

        public void Update(string question, string answer, int order)
        {
            Question = question;
            Answer = answer;
            Order = order;
        }
    }
}
