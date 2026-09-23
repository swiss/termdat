using Bk.Termdat.Business.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bk.Termdat.Infrastructure.DataSource.Configuration;

public class DomainConfiguration : EntityChangeTrackingConfiguration<Domain>
{
    protected override void ConfigureEntityChangeTracking(EntityTypeBuilder<Domain> builder)
    {
        builder.HasOne(d => d.Subject)
            .WithMany(s => s.Domains)
            .HasForeignKey(d => d.SubjectId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.IsMultiLingual<Domain, DomainTranslation>();
    }
}
