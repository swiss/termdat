using Bk.Termdat.Business.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bk.Termdat.Infrastructure.DataSource.Configuration;

public class LenochClassificationSubjectDomainMappingConfiguration : IEntityTypeConfiguration<LenochClassificationSubjectDomainMapping>
{
    public void Configure(EntityTypeBuilder<LenochClassificationSubjectDomainMapping> builder)
    {
        builder.HasKey(m => m.Id);

        // Ef Core Migrations were created with this singular 'mapping', but queries for the table are done using plural 'mappings', which results
        // in an exception of relation: lenoch_classification_subject_domain_mappings does not exist
        // this is the fix for this exception
        builder.ToTable("lenoch_classification_subject_domain_mapping");

        builder.HasOne(m => m.LenochClassification)
            .WithMany(lc => lc.LenochClassificationSubjectDomainMappings)
            .HasForeignKey(m => m.LenochClassificationId);

        builder.HasOne(m => m.Subject)
            .WithMany(s => s.LenochClassificationSubjectDomainMappings)
            .HasForeignKey(m => m.SubjectId);

        builder.HasOne(m => m.Domain)
            .WithMany(d => d.LenochClassificationSubjectDomainMappings)
            .HasForeignKey(m => m.DomainId)
            .IsRequired(false);
    }
}
