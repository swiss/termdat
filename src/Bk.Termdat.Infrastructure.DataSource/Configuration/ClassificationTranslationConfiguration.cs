using System;
using Bk.Termdat.Business.Domain.Models;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bk.Termdat.Infrastructure.DataSource.Configuration;

public class ClassificationTranslationConfiguration : EntityConfiguration<ClassificationTranslation>
{
    protected override void ConfigureEntity(EntityTypeBuilder<ClassificationTranslation> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.IsTranslation();

        builder.Property(ct => ct.Name)
            .HasMaxLength(1000)
            .IsRequired();

        builder.HasOne(dt => dt.Classification)
            .WithMany(d => d.Translations)
            .HasForeignKey(dt => dt.ClassificationId);
    }
}
