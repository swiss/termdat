using Bk.Termdat.Business.Domain.Models;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bk.Termdat.Infrastructure.DataSource.Configuration;

public class LenochClassificationTranslationConfiguration : EntityConfiguration<LenochClassificationTranslation>
{
    protected override void ConfigureEntity(EntityTypeBuilder<LenochClassificationTranslation> builder)
    {
        builder.IsTranslation();

        builder.Property(ct => ct.Name)
            .HasMaxLength(1000)
            .IsRequired();

        builder.HasOne(dt => dt.LenochClassification)
            .WithMany(d => d.Translations)
            .HasForeignKey(dt => dt.LenochClassificationId);
    }
}
