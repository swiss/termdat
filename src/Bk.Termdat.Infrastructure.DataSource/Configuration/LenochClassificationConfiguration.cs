using Bk.Termdat.Business.Domain.Models;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bk.Termdat.Infrastructure.DataSource.Configuration;

public class LenochClassificationConfiguration : EntityChangeTrackingConfiguration<LenochClassification>
{
    protected override void ConfigureEntityChangeTracking(EntityTypeBuilder<LenochClassification> builder)
    {
        builder.Property(c => c.Code)
            .HasMaxLength(100)
            .IsRequired();

        builder.HasIndex(u => u.Code)
            .IsUnique();

        builder.IsMultiLingual<LenochClassification, LenochClassificationTranslation>();
    }
}
