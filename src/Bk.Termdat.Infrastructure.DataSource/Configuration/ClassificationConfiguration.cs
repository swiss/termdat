using System;
using Bk.Termdat.Business.Domain.Models;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bk.Termdat.Infrastructure.DataSource.Configuration;

public class ClassificationConfiguration : EntityChangeTrackingConfiguration<Classification>
{
    protected override void ConfigureEntityChangeTracking(EntityTypeBuilder<Classification> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Property(c => c.Code)
            .HasMaxLength(100)
            .IsRequired();

        builder.IsMultiLingual<Classification, ClassificationTranslation>();
    }
}
