using System;
using Bk.Termdat.Business.Domain.Models;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bk.Termdat.Infrastructure.DataSource.Configuration;

public class NewsItemConfiguration : EntityChangeTrackingConfiguration<NewsItem>
{
    protected override void ConfigureEntityChangeTracking(EntityTypeBuilder<NewsItem> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Property(ni => ni.ValidFrom)
           .IsRequired();

        builder.Property(ni => ni.ValidTo)
            .IsRequired();
    }
}
