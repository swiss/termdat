using Bk.Termdat.Business.Domain.Models;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bk.Termdat.Infrastructure.DataSource.Configuration;

public class NewsItemConfiguration : EntityConfiguration<NewsItem>
{
    protected override void ConfigureEntity(EntityTypeBuilder<NewsItem> builder)
    {
        builder.Property(ni => ni.ValidFrom)
           .IsRequired();

        builder.Property(ni => ni.ValidTo)
            .IsRequired();
    }
}
