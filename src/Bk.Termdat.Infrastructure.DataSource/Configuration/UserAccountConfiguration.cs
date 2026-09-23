using Bk.Termdat.Business.Domain.Models;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bk.Termdat.Infrastructure.DataSource.Configuration;

public class UserAccountConfiguration : EntityChangeTrackingConfiguration<UserAccount>
{
    protected override void ConfigureEntityChangeTracking(EntityTypeBuilder<UserAccount> builder)
    {
        builder.Property(u => u.Email)
            .HasMaxLength(100)
            .IsRequired();

        builder.HasIndex(u => u.Email)
            .IsUnique();

        builder.Property(u => u.LastName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(u => u.FirstName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(u => u.Role)
            .HasMaxLength(100);

        builder.Property(u => u.Information)
            .HasMaxLength(1000);

        builder.HasMany(u => u.AuthorizedGlossaries)
            .WithMany(g => g.UserAccounts);

        builder.HasMany(u => u.AuthorizedCollections)
            .WithMany(c => c.UserAccounts);
    }
}

