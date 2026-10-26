using System;
using Bk.Termdat.Business.Domain.Models;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bk.Termdat.Infrastructure.DataSource.Configuration;

public class OfficeConfiguration : EntityChangeTrackingConfiguration<Office>
{
    protected override void ConfigureEntityChangeTracking(EntityTypeBuilder<Office> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Property(c => c.Code)
            .HasMaxLength(5)
            .IsRequired();

        builder.HasIndex(c => c.Code)
            .IsUnique();

        builder.IsMultiLingual<Office, OfficeTranslation>();
    }
}
