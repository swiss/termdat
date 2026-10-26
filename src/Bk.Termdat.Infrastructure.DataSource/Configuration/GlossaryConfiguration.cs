using System;
using Bk.Termdat.Business.Domain.Models;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bk.Termdat.Infrastructure.DataSource.Configuration;

public class GlossaryConfiguration : EntityChangeTrackingConfiguration<Glossary>
{
    protected override void ConfigureEntityChangeTracking(EntityTypeBuilder<Glossary> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Property(g => g.Code)
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(g => g.Description)
            .HasMaxLength(1000);

        builder.HasIndex(g => g.Code)
            .IsUnique();

        builder.IsMultiLingual<Glossary, GlossaryTranslation>();
    }
}
