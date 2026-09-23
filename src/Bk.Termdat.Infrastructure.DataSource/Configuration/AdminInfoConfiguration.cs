using Bk.Termdat.Business.Domain.Models;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bk.Termdat.Infrastructure.DataSource.Configuration;

public class AdminInfoConfiguration : EntityChangeTrackingConfiguration<AdminInfo>
{
    protected override void ConfigureEntityChangeTracking(EntityTypeBuilder<AdminInfo> builder)
    {
        builder.Property(a => a.Code)
        .HasMaxLength(100)
        .IsRequired();

        builder.HasIndex(a => a.Code)
            .IsUnique();

        builder.Property(a => a.Description)
            .HasMaxLength(1000);
    }
}
