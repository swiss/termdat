using Bk.Termdat.Business.Domain.Models;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bk.Termdat.Infrastructure.DataSource.Configuration;

public abstract class EntityChangeTrackingConfiguration<T> : EntityConfiguration<T>
    where T : EntityChangeTracking
{
    protected override void ConfigureEntity(EntityTypeBuilder<T> builder)
    {
        builder.Property(e => e.CreatedBy)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(e => e.ModifiedBy)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(e => e.Created)
            .IsRequired();

        builder.Property(e => e.Modified)
            .IsRequired();

        ConfigureEntityChangeTracking(builder);
    }

    protected abstract void ConfigureEntityChangeTracking(EntityTypeBuilder<T> builder);
}
