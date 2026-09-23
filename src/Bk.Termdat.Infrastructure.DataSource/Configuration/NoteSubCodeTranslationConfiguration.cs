using Bk.Termdat.Business.Domain.Models;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bk.Termdat.Infrastructure.DataSource.Configuration;

public class NoteSubCodeTranslationConfiguration : EntityConfiguration<NoteSubCodeTranslation>
{
    protected override void ConfigureEntity(EntityTypeBuilder<NoteSubCodeTranslation> builder)
    {
        builder.IsTranslation();

        builder.Property(ct => ct.Name)
            .HasMaxLength(1000)
            .IsRequired();

        builder.HasOne(ct => ct.NoteSubCode)
            .WithMany(c => c.Translations)
            .HasForeignKey(ct => ct.NoteSubCodeId);
    }
}
