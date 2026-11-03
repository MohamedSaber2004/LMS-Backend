using LMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LMS.Persistence.Configurations
{
    public sealed class HelpArticleConfiguration : IEntityTypeConfiguration<HelpArticle>
    {
        public void Configure(EntityTypeBuilder<HelpArticle> builder)
        {
            builder.ToTable("HelpArticles");

            builder.HasKey(article => article.Id);

            builder.Property(article => article.Question)
                .HasMaxLength(300)
                .IsRequired();

            // Answer stays nvarchar(max): help articles are long-form plain text.
            builder.Property(article => article.Answer)
                .IsRequired();

            builder.Property(article => article.Order)
                .IsRequired();

            builder.Property(article => article.CreatedBy)
                .IsRequired();

            builder.Property(article => article.Version)
                .IsRowVersion();

            builder.HasIndex(article => article.CategoryId);
            builder.HasIndex(article => new { article.CategoryId, article.Order });
            builder.HasIndex(article => article.IsDeleted);

            builder.HasOne(article => article.Category)
                .WithMany(category => category.Articles)
                .HasForeignKey(article => article.CategoryId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasQueryFilter(article => !article.IsDeleted);
        }
    }
}
