using Bk.Termdat.Business.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bk.Termdat.Infrastructure.DataSource.Configuration;

public class EntryConfiguration : EntityChangeTrackingConfiguration<Entry>
{
    protected override void ConfigureEntityChangeTracking(EntityTypeBuilder<Entry> builder)
    {
        builder.Property(e => e.Status)
            .HasConversion<string>()
            .HasMaxLength(256)
            .IsRequired()
            .HasDefaultValue(Status.InProgress);

        builder.Property(e => e.Reliability)
            .HasConversion<string>()
            .HasMaxLength(256)
            .IsRequired()
            .HasDefaultValue(Reliability.FirstDraft);

        builder.Property(e => e.Identification)
            .IsRequired(false);

        builder.Property(e => e.Comment)
            .HasMaxLength(4000);

        builder.Property(e => e.AdminInfo)
            .HasMaxLength(450);

        builder.Property(e => e.LockedBy)
            .HasMaxLength(100);

        builder.IsMultiLingual<Entry, EntryLanguage>();

        builder.HasOne(e => e.ConceptSystemPosition)
            .WithMany(c => c.Entries)
            .HasForeignKey(e => e.ConceptSystemPositionId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(e => e.Collection)
            .WithMany(c => c.Entries)
            .HasForeignKey(e => e.CollectionId);

        builder.HasOne(e => e.Office)
            .WithMany(o => o.Entries)
            .HasForeignKey(e => e.OfficeId);

        builder.HasMany(e => e.LenochClassifications)
            .WithMany(f => f.Entries);

        builder.HasMany(e => e.RelatedCollections)
            .WithMany(c => c.RelatedEntries);

        builder.HasMany(e => e.Glossaries)
            .WithMany(g => g.Entries);

        builder.HasMany(e => e.Subjects)
            .WithMany(c => c.Entries);

        builder.HasMany(e => e.Domains)
            .WithMany(c => c.Entries);

        builder.HasMany(e => e.AdminInfos)
            .WithMany(c => c.Entries);

        builder.Property(m => m.ExternalSystemId)
            .HasMaxLength(100)
            .IsRequired(false);
    }
}
