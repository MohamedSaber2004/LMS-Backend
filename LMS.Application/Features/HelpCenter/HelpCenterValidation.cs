using LMS.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LMS.Application.Features.HelpCenter
{
    internal static class HelpCenterValidation
    {
        public static async Task<bool> CategoryExistsAsync(
            ILmsDbContext context,
            Guid categoryId,
            CancellationToken cancellationToken)
        {
            return categoryId != Guid.Empty
                && await context.HelpCategories.AnyAsync(
                    category => category.Id == categoryId, cancellationToken);
        }

        /// <summary>
        /// Soft-deleted categories are already hidden by the query filter, so a deleted row never
        /// keeps its name reserved.
        /// </summary>
        public static async Task<bool> CategoryNameIsAvailableAsync(
            ILmsDbContext context,
            string? name,
            Guid? excludedCategoryId,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return true;
            }

            var trimmedName = name.Trim();

            var query = context.HelpCategories
                .Where(category => category.Name == trimmedName);

            if (excludedCategoryId.HasValue)
            {
                var excludedId = excludedCategoryId.Value;
                query = query.Where(category => category.Id != excludedId);
            }

            return !await query.AnyAsync(cancellationToken);
        }
    }
}
