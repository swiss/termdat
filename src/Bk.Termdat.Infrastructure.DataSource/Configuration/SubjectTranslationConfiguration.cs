using System;
using Bk.Termdat.Business.Domain.Models;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bk.Termdat.Infrastructure.DataSource.Configuration;

public class SubjectTranslationConfiguration : EntityConfiguration<SubjectTranslation>
{
    protected override void ConfigureEntity(EntityTypeBuilder<SubjectTranslation> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.IsTranslation();

        builder.Property(st => st.Name)
            .HasMaxLength(1000)
            .IsRequired();

        builder.HasOne(st => st.Subject)
            .WithMany(s => s.Translations)
            .HasForeignKey(st => st.SubjectId);
    }
}
