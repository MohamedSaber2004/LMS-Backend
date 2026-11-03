using LMS.Application.Features.HelpCenter.DTOs;
using LMS.Domain.Entities;

namespace LMS.Application.Features.HelpCenter
{
    public static class HelpCenterMapper
    {
        /// <summary>
        /// Expects the <c>Articles</c> collection to be loaded; the soft-delete query filter
        /// already hides deleted articles, so only <c>IsActive</c> has to be checked.
        /// </summary>
        public static HelpCategoryDto ToCategoryDto(HelpCategory category) => new()
        {
            Id = category.Id,
            Name = category.Name,
            Order = category.Order,
            IsActive = category.IsActive,
            ArticleCount = category.Articles.Count(article => article.IsActive)
        };

        public static HelpArticleDto ToArticleDto(HelpArticle article)
            => ToArticleDto(article, article.Category?.Name ?? string.Empty);

        /// <summary>
        /// Used when the <c>Category</c> navigation was not loaded (e.g. right after an insert).
        /// </summary>
        public static HelpArticleDto ToArticleDto(HelpArticle article, string categoryName) => new()
        {
            Id = article.Id,
            CategoryId = article.CategoryId,
            CategoryName = categoryName,
            Question = article.Question,
            Answer = article.Answer,
            Order = article.Order,
            IsActive = article.IsActive,
            CreatedAt = article.CreatedAt,
            UpdatedAt = article.UpdatedAt
        };
    }
}
