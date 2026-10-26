using System;
using Bk.Termdat.Business.Domain.Models;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bk.Termdat.Infrastructure.DataSource.Configuration;

public class EntryTemplateConfiguration : EntityChangeTrackingConfiguration<EntryTemplate>
{
    protected override void ConfigureEntityChangeTracking(EntityTypeBuilder<EntryTemplate> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(e => new { e.Name, e.CreatedBy })
            .IsUnique();

        builder.Property(e => e.Template)
            .IsRequired();
    }
}
