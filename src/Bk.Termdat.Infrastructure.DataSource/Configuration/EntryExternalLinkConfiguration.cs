using Bk.Termdat.Business.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bk.Termdat.Infrastructure.DataSource.Configuration;

public class EntryExternalLinkConfiguration : IEntityTypeConfiguration<EntryExternalLink>
{
    public void Configure(EntityTypeBuilder<EntryExternalLink> builder)
    {
        builder.HasKey(er => er.Id);

        builder.Property(er => er.Type)
            .HasConversion<string>()
            .HasMaxLength(256)
            .IsRequired();

        builder.HasOne(er => er.Entry)
            .WithMany(e => e.ExternalLinks)
            .HasForeignKey(er => er.EntryId)
            .IsRequired();
    }
}
