using System;
using Bk.Termdat.Business.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bk.Termdat.Infrastructure.DataSource.Configuration;

public class EntryReferenceConfiguration : IEntityTypeConfiguration<EntryReference>
{
    public void Configure(EntityTypeBuilder<EntryReference> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.HasKey(er => er.Id);

        builder.Property(er => er.Type)
            .HasConversion<string>()
            .HasMaxLength(256)
            .IsRequired();

        builder.HasOne(er => er.ReferencedEntry)
            .WithMany(e => e.ReferencedByEntries)
            .HasForeignKey(er => er.ReferencedEntryId)
            .IsRequired();

        builder.HasOne(er => er.Entry)
            .WithMany(e => e.References)
            .HasForeignKey(er => er.EntryId)
            .IsRequired();
    }
}
