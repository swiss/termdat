using Bk.Termdat.Business.Domain.Models;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bk.Termdat.Infrastructure.DataSource.Configuration;

public class SubjectConfiguration : EntityChangeTrackingConfiguration<Subject>
{
    protected override void ConfigureEntityChangeTracking(EntityTypeBuilder<Subject> builder)
    {
        builder.IsMultiLingual<Subject, SubjectTranslation>();
    }
}
