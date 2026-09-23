using Bk.Termdat.Business.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Bk.Termdat.Infrastructure.DataSource;

public class DataContext : DbContext
{
    public const string Schema = "data";

    public DataContext(DbContextOptions<DataContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DataContext).Assembly);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSnakeCaseNamingConvention();
        // https://github.com/dotnet/efcore/issues/23803
        optionsBuilder.ConfigureWarnings(w => w.Ignore(CoreEventId.RowLimitingOperationWithoutOrderByWarning));
    }

    public DbSet<Language> Languages { get; set; }
    public DbSet<LanguageTranslation> LanguageTranslations { get; set; }
    public DbSet<Entry> Entries { get; set; }
    public DbSet<EntryLanguage> EntryLanguages { get; set; }
    public DbSet<EntryReference> EntryReferences { get; set; }
    public DbSet<EntryExternalLink> EntryExternalLinks { get; set; }
    public DbSet<Collection> Collections { get; set; }
    public DbSet<CollectionTranslation> CollectionTranslations { get; set; }
    public DbSet<Office> Offices { get; set; }
    public DbSet<OfficeTranslation> OfficeTranslations { get; set; }
    public DbSet<Domain> Domains { get; set; }
    public DbSet<DomainTranslation> DomainTranslations { get; set; }
    public DbSet<Subject> Subjects { get; set; }
    public DbSet<SubjectTranslation> SubjectTranslations { get; set; }
    public DbSet<LenochClassification> LenochClassifications { get; set; }
    public DbSet<LenochClassificationTranslation> LenochClassificationTranslations { get; set; }
    public DbSet<LenochClassificationSubjectDomainMapping> LenochClassificationSubjectDomainMappings { get; set; }
    public DbSet<Classification> Classifications { get; set; }
    public DbSet<ClassificationTranslation> ClassificationTranslations { get; set; }
    public DbSet<UserAccount> UserAccounts { get; set; }
    public DbSet<Picture> Pictures { get; set; }
    public DbSet<PictureTranslation> PictureTranslations { get; set; }
    public DbSet<PictureFile> PictureFiles { get; set; }
    public DbSet<ConceptSystem> ConceptSystems { get; set; }
    public DbSet<ConceptSystemPosition> ConceptSystemPositions { get; set; }
    public DbSet<Glossary> Glossaries { get; set; }
    public DbSet<GlossaryTranslation> GlossaryTranslations { get; set; }
    public DbSet<NoteSubCode> NoteSubCodes { get; set; }
    public DbSet<NoteSubCodeTranslation> NoteSubCodeTranslations { get; set; }
    public DbSet<AdminInfo> AdminInfos { get; set; }
    public DbSet<Statistic> Statistics { get; set; }
    public DbSet<EntryTemplate> EntryTemplates { get; set; }
    public DbSet<SourceTemplate> SourceTemplates { get; set; }
    public DbSet<NewsItem> NewsItems { get; set; }
    public DbSet<NewsItemTranslation> NewsItemTranslations { get; set; }
}
