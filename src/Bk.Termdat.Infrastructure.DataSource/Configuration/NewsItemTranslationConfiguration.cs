using System;
using Bk.Termdat.Business.Domain.Models;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bk.Termdat.Infrastructure.DataSource.Configuration;

public class NewsItemTranslationConfiguration : EntityConfiguration<NewsItemTranslation>
{
    protected override void ConfigureEntity(EntityTypeBuilder<NewsItemTranslation> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Property(nt => nt.Title)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(nt => nt.Text)
            .IsRequired()
            .HasMaxLength(4000);

        builder.HasOne(nt => nt.NewsItem)
            .WithMany(n => n.Translations)
            .HasForeignKey(nt => nt.NewsItemId);

        builder.HasOne(nt => nt.Language)
            .WithMany()
            .HasForeignKey(nt => nt.LanguageId);

        builder.HasIndex(nt => new { nt.NewsItemId, nt.LanguageId })
            .IsUnique();
    }
}
