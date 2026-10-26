using System;
using Bk.Termdat.Business.Domain.Models;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bk.Termdat.Infrastructure.DataSource.Configuration;

public class ConceptSystemPositionConfiguration : EntityConfiguration<ConceptSystemPosition>
{
    protected override void ConfigureEntity(EntityTypeBuilder<ConceptSystemPosition> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Property(cs => cs.Value)
            .HasMaxLength(1000);

        builder.HasOne(cs => cs.ConceptSystem)
            .WithMany(c => c.ConceptSystemPositions)
            .HasForeignKey(cs => cs.ConceptSystemId)
            .IsRequired();
    }
}
