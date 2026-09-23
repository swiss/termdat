using Bk.Termdat.Business.Domain.Models;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bk.Termdat.Infrastructure.DataSource.Configuration;

public class PictureTranslationConfiguration : EntityConfiguration<PictureTranslation>
{
    protected override void ConfigureEntity(EntityTypeBuilder<PictureTranslation> builder)
    {
        builder.Property(pt => pt.Description)
            .HasMaxLength(2000)
            .IsRequired(false);

        builder.Property(pt => pt.Source)
            .HasMaxLength(2000)
            .IsRequired(false);

        builder.HasOne(pt => pt.Picture)
            .WithMany(d => d.Translations)
            .HasForeignKey(pt => pt.EntryId);

        builder.HasOne(pt => pt.Language);
    }
}
