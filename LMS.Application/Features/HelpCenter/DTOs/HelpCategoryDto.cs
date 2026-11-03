namespace LMS.Application.Features.HelpCenter.DTOs
{
    public class HelpCategoryDto
    {
        public HelpCategoryDto() { }

        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Order { get; set; }
        public bool IsActive { get; set; }

        /// <summary>
        /// Number of articles that are still active inside this category.
        /// </summary>
        public int ArticleCount { get; set; }
    }
}
