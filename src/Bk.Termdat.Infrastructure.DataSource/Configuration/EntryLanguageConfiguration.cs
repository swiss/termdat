using Bk.Termdat.Business.Domain.Models;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bk.Termdat.Infrastructure.DataSource.Configuration;

public class EntryLanguageConfiguration : EntityConfiguration<EntryLanguage>
{
    protected override void ConfigureEntity(EntityTypeBuilder<EntryLanguage> builder)
    {
        builder.IsTranslation();

        builder.Property(et => et.Sequence)
            .IsRequired();
        builder.Property(et => et.Terminus)
            .HasMaxLength(4000);
        builder.Property(et => et.TerminusSource)
            .HasMaxLength(4000);
        builder.Property(et => et.Name)
            .HasMaxLength(4000);
        builder.Property(et => et.NameSource)
            .HasMaxLength(4000);
        builder.Property(et => et.Abbreviation)
            .HasMaxLength(4000);
        builder.Property(et => et.AbbreviationSource)
            .HasMaxLength(4000);
        builder.Property(et => et.Phraseology)
            .HasMaxLength(4000);
        builder.Property(et => et.PhraseologySource)
            .HasMaxLength(4000);
        builder.Property(et => et.Definition)
            .HasMaxLength(4000);
        builder.Property(et => et.DefinitionSource)
            .HasMaxLength(4000);
        builder.Property(et => et.Note)
            .HasMaxLength(4000);
        builder.Property(et => et.NoteSource)
            .HasMaxLength(4000);
        builder.Property(et => et.Context)
            .HasMaxLength(4000);
        builder.Property(et => et.ContextSource)
            .HasMaxLength(4000);
        builder.Property(et => et.Country)
            .HasMaxLength(4000);
        builder.Property(et => et.Metadata)
            .HasMaxLength(4000);
        builder.Property(et => et.MetadataSource)
            .HasMaxLength(4000);
        builder.Ignore(et => et.IsSynonym);

        builder.HasOne(el => el.Language)
            .WithMany()
            .HasForeignKey(el => el.LanguageId);

        builder.HasOne(el => el.Entry)
            .WithMany(e => e.Translations)
            .HasForeignKey(el => el.EntryId);

        builder.Property(m => m.ExternalSystemId)
            .HasMaxLength(100)
            .IsRequired(false);
    }
}
