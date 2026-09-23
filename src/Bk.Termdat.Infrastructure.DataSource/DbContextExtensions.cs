using Bk.Termdat.Business.Domain.Models;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bk.Termdat.Infrastructure.DataSource;

public static class DbContextExtensions
{
    public static void IsMultiLingual<TMultiLingualEntity,
        TTranslation>(this EntityTypeBuilder<TMultiLingualEntity> modelBuilder)
        where TMultiLingualEntity : class, IMultiLingualEntity<TTranslation>
        where TTranslation : class, IEntityTranslation
    {
        modelBuilder.HasMany(mle => mle.Translations);
    }

    public static void IsTranslation<TEntityTranslation>(this EntityTypeBuilder<TEntityTranslation> modelBuilder)
        where TEntityTranslation : Entity, IEntityTranslation
    {
        modelBuilder.HasOne(et => et.Language);
    }
}
