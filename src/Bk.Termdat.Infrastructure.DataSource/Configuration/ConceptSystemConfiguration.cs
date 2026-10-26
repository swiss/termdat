using System;
using Bk.Termdat.Business.Domain.Models;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bk.Termdat.Infrastructure.DataSource.Configuration;

public class ConceptSystemConfiguration : EntityChangeTrackingConfiguration<ConceptSystem>
{
    protected override void ConfigureEntityChangeTracking(EntityTypeBuilder<ConceptSystem> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Property(ct => ct.Title)
            .HasMaxLength(1000)
            .IsRequired();

        builder.HasOne(cs => cs.Collection)
            .WithOne(c => c.ConceptSystem)
            .HasForeignKey<ConceptSystem>(cs => cs.CollectionId)
            .IsRequired();
    }
}
