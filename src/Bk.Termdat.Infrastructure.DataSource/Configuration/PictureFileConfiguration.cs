using System;
using Bk.Termdat.Business.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bk.Termdat.Infrastructure.DataSource.Configuration;

public class PictureFileConfiguration : IEntityTypeConfiguration<PictureFile>
{

    public void Configure(EntityTypeBuilder<PictureFile> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.HasKey(p => p.EntryId);

        builder.Property(p => p.Value)
            .IsRequired();

        builder.HasOne(p => p.Picture)
            .WithOne(p => p.File)
            .HasForeignKey<PictureFile>(p => p.EntryId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
