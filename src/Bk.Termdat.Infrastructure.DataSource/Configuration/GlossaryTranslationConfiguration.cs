using Bk.Termdat.Business.Domain.Models;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bk.Termdat.Infrastructure.DataSource.Configuration;

public class GlossaryTranslationConfiguration : EntityConfiguration<GlossaryTranslation>
{
    protected override void ConfigureEntity(EntityTypeBuilder<GlossaryTranslation> builder)
    {
        builder.IsTranslation();

        builder.Property(ct => ct.Name)
            .HasMaxLength(1000)
            .IsRequired();

        builder.HasOne(ct => ct.Glossary)
            .WithMany(c => c.Translations)
            .HasForeignKey(ct => ct.GlossaryId);
    }
}
