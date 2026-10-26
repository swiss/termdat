using System;
using Bk.Termdat.Business.Domain.Models;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bk.Termdat.Infrastructure.DataSource.Configuration;

public class CollectionTranslationConfiguration : EntityConfiguration<CollectionTranslation>
{
    protected override void ConfigureEntity(EntityTypeBuilder<CollectionTranslation> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.IsTranslation();

        builder.Property(ct => ct.Name)
            .HasMaxLength(1000)
            .IsRequired();

        builder.HasOne(ct => ct.Collection)
            .WithMany(c => c.Translations)
            .HasForeignKey(ct => ct.CollectionId);
    }
}
