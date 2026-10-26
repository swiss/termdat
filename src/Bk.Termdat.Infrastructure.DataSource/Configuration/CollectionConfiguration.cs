using System;
using Bk.Termdat.Business.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bk.Termdat.Infrastructure.DataSource.Configuration;

public class CollectionConfiguration : EntityChangeTrackingConfiguration<Collection>
{
    protected override void ConfigureEntityChangeTracking(EntityTypeBuilder<Collection> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Property(c => c.Code)
            .HasMaxLength(10)
            .IsRequired();

        builder.HasIndex(c => c.Code)
            .IsUnique();

        builder.Property(c => c.Documentation)
            .HasMaxLength(1000);

        builder.Property(c => c.Description)
            .HasMaxLength(1000);

        builder.HasOne(c => c.Classification)
            .WithMany(c => c.Collections)
            .HasForeignKey(c => c.ClassificationId)
            .IsRequired(false);

        builder.Property(c => c.ExportLindas)
            .HasDefaultValue(false);

        builder.IsMultiLingual<Collection, CollectionTranslation>();
    }
}
