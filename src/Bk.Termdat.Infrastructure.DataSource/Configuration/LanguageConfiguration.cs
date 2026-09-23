using Bk.Termdat.Business.Domain.Models;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bk.Termdat.Infrastructure.DataSource.Configuration;

public class LanguageConfiguration : EntityChangeTrackingConfiguration<Language>
{
    protected override void ConfigureEntityChangeTracking(EntityTypeBuilder<Language> builder)
    {
        builder.Property(l => l.IsoCode)
            .HasMaxLength(2)
            .IsRequired();

        builder.HasIndex(l => l.IsoCode)
            .IsUnique();

        builder.IsMultiLingual<Language, LanguageTranslation>();
    }
}
