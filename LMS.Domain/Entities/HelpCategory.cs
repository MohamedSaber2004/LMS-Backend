using LMS.Domain.Common;

namespace LMS.Domain.Entities
{
    public class HelpCategory : BaseEntity<Guid>
    {
        public string Name { get; private set; } = string.Empty;
        public int Order { get; private set; }

        public virtual ICollection<HelpArticle> Articles { get; private set; } = new List<HelpArticle>();

        public static HelpCategory Create(string name, int order)
        {
            return new HelpCategory
            {
                Name = name,
                Order = order
            };
        }

        public void Update(string name, int order)
        {
            Name = name;
            Order = order;
        }
    }
}
