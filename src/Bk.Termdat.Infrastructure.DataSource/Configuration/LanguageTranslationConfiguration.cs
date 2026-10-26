using System;
using Bk.Termdat.Business.Domain.Models;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bk.Termdat.Infrastructure.DataSource.Configuration;

public class LanguageTranslationConfiguration : EntityConfiguration<LanguageTranslation>
{
    protected override void ConfigureEntity(EntityTypeBuilder<LanguageTranslation> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Property(lt => lt.Name)
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(l => l.IsoCode)
            .HasMaxLength(2)
            .IsRequired();

        builder.HasOne(lt => lt.Language)
            .WithMany(l => l.Translations)
            .HasForeignKey(lt => lt.LanguageId);
    }
}
