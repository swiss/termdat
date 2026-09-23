using Bk.Termdat.Business.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bk.Termdat.Infrastructure.DataSource.Configuration;

public class PictureConfiguration : IEntityTypeConfiguration<Picture>
{
    public void Configure(EntityTypeBuilder<Picture> builder)
    {
        builder.HasKey(p => p.EntryId);

        builder.Property(p => p.ContentType)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(p => p.FileExtension)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(p => p.EntityTag)
            .HasMaxLength(100)
            .IsRequired();

        builder.HasMany(p => p.Translations)
            .WithOne(t => t.Picture)
            .HasForeignKey(p => p.EntryId);

        builder.HasOne(p => p.Entry)
            .WithOne(e => e.Picture)
            .HasForeignKey<Picture>(p => p.EntryId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
