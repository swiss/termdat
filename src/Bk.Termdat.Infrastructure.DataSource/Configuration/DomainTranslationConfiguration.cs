using System;
using Bk.Termdat.Business.Domain.Models;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bk.Termdat.Infrastructure.DataSource.Configuration;

public class DomainTranslationConfiguration : EntityConfiguration<DomainTranslation>
{
    protected override void ConfigureEntity(EntityTypeBuilder<DomainTranslation> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.IsTranslation();

        builder.Property(dt => dt.Name)
            .HasMaxLength(1000)
            .IsRequired();

        builder.HasOne(dt => dt.Domain)
            .WithMany(d => d.Translations)
            .HasForeignKey(dt => dt.DomainId);
    }
}
