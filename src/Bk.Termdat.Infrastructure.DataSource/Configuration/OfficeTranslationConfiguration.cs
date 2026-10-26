using System;
using Bk.Termdat.Business.Domain.Models;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bk.Termdat.Infrastructure.DataSource.Configuration;

public class OfficeTranslationConfiguration : EntityConfiguration<OfficeTranslation>
{
    protected override void ConfigureEntity(EntityTypeBuilder<OfficeTranslation> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.IsTranslation();

        builder.Property(ot => ot.Name)
            .HasMaxLength(1000)
            .IsRequired();

        builder.HasOne(ot => ot.Office)
            .WithMany(o => o.Translations)
            .HasForeignKey(ot => ot.OfficeId);
    }
}
