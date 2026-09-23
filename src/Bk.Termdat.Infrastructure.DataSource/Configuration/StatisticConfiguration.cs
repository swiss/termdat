using Bk.Termdat.Business.Domain.Models;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bk.Termdat.Infrastructure.DataSource.Configuration;

public class StatisticConfiguration : EntityChangeTrackingConfiguration<Statistic>
{
    protected override void ConfigureEntityChangeTracking(EntityTypeBuilder<Statistic> builder)
    {
        builder.Property(s => s.SearchText)
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(s => s.Languages)
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(a => a.SearchFields)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(s => s.NumberOfSearches)
            .IsRequired();
    }
}
