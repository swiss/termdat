using Bk.Termdat.Business.Domain.Models;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bk.Termdat.Infrastructure.DataSource.Configuration;

public class NoteSubCodeConfiguration : EntityChangeTrackingConfiguration<NoteSubCode>
{
    protected override void ConfigureEntityChangeTracking(EntityTypeBuilder<NoteSubCode> builder)
    {
        builder.Property(g => g.Code)
            .HasMaxLength(10)
            .IsRequired();

        builder.HasIndex(g => g.Code)
            .IsUnique();

        builder.IsMultiLingual<NoteSubCode, NoteSubCodeTranslation>();
    }
}

