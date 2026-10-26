using System;
using Bk.Termdat.Business.Domain.Models;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bk.Termdat.Infrastructure.DataSource.Configuration;

public class SourceTemplateConfiguration : EntityChangeTrackingConfiguration<SourceTemplate>
{
    protected override void ConfigureEntityChangeTracking(EntityTypeBuilder<SourceTemplate> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Property(et => et.TextDe)
        .HasMaxLength(4000);

        builder.Property(et => et.TextFr)
        .HasMaxLength(4000);

        builder.Property(et => et.TextIt)
        .HasMaxLength(4000);

        builder.Property(et => et.TextEn)
        .HasMaxLength(4000);

        builder.Property(et => et.TextRm)
        .HasMaxLength(4000);

        builder.HasOne(e => e.User)
       .WithMany()
       .HasForeignKey(e => e.UserId);
    }
}
