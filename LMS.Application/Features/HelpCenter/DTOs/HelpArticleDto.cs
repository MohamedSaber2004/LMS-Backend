namespace LMS.Application.Features.HelpCenter.DTOs
{
    public class HelpArticleDto
    {
        public HelpArticleDto() { }

        public Guid Id { get; set; }
        public Guid CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string Question { get; set; } = string.Empty;

        /// <summary>
        /// Plain text. Newlines are preserved; the front end is responsible for rendering them.
        /// </summary>
        public string Answer { get; set; } = string.Empty;

        public int Order { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
