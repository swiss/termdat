using Bk.Termdat.Business.Clients;
using Bk.Termdat.Business.Domain;
using Bk.Termdat.Business.Mapping;
using Bk.Termdat.Business.Repositories;
using Bk.Termdat.Business.Services;
using Bk.Termdat.Business.Services.Interfaces;
using Bk.Termdat.CrossCutting;
using Bk.Termdat.Infrastructure.DataSource;
using Bk.Termdat.Infrastructure.DataSource.Repositories;
using Bk.Termdat.Infrastructure.Service;
using Bk.Termdat.Private.Api.HostedServices;
using Microsoft.Extensions.DependencyInjection;

namespace Bk.Termdat.Private.Api;

internal static class ServiceCollectionExtension
{
    internal static void AddTermdat(this IServiceCollection services)
    {
        services.AddSingleton<ICultureService, CultureService>();
        services.AddSingleton<IPictureTranslationMapper, PictureTranslationMapper>();
        services.AddSingleton<ILanguageTranslationMapper, LanguageTranslationMapper>();
        services.AddSingleton<IEntryNotePatternService, EntryNotePatternService>();
        services.AddSingleton<IBackgroundServiceScheduler, BackgroundServiceScheduler>();
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();

        services.AddScoped<ISearchQueryService, SearchQueryService>();
        services.AddScoped<ILanguageService, LanguageService>();
        services.AddScoped<IOfficeService, OfficeService>();
        services.AddScoped<ILenochClassificationService, LenochClassificationService>();
        services.AddScoped<ICollectionService, CollectionService>();
        services.AddScoped<ICollectionValidationService, CollectionValidationService>();
        services.AddScoped<IConceptSystemService, ConceptSystemService>();
        services.AddScoped<IClassificationService, ClassificationService>();
        services.AddScoped<IUserAccountService, UserAccountService>();
        services.AddScoped<IAdminInfoService, AdminInfoService>();
        services.AddScoped<IEntryService, EntryService>();
        services.AddScoped<IEntryHeaderUpdateService, EntryHeaderUpdateService>();
        services.AddScoped<IEntryValidationService, EntryValidationService>();
        services.AddScoped<IUserAccountValidationService, UserAccountValidationService>();
        services.AddScoped<IConceptSystemPositionService, ConceptSystemPositionService>();
        services.AddScoped<IConceptSystemRepository, ConceptSystemRepository>();
        services.AddScoped<IPictureService, PictureService>();
        services.AddScoped<IWatchlistService, WatchlistService>();
        services.AddScoped<IGlossaryService, GlossaryService>();
        services.AddScoped<ISubjectService, SubjectService>();
        services.AddScoped<ISourceTemplateService, SourceTemplateService>();
        services.AddScoped<IDomainService, DomainService>();
        services.AddScoped<ILanguageDescriptionMapper, LanguageDescriptionMapper>();
        services.AddScoped<ILanguageNewsTranslationMapper, LanguageNewsTranslationMapper>();
        services.AddScoped<ITermdatUrlService, TermdatUrlService>();
        services.AddScoped<IStatisticService, StatisticService>();
        services.AddScoped<ISearchStatisticService, SearchStatisticService>();
        services.AddScoped<ITbxDataService, TbxDataService>();
        services.AddScoped<IExcelDataService, ExcelDataService>();
        services.AddScoped<IFileImportService, FileImportService>();
        services.AddScoped<IEntryTemplateService, EntryTemplateService>();
        services.AddScoped<INoteSubCodeTranslationService, NoteSubCodeTranslationService>();
        services.AddScoped<IDuplicateCheckService, DuplicateCheckService>();
        services.AddScoped<INewsService, NewsService>();

        services.AddScoped<AuthorizationService>();
        services.AddScoped<OverrideableAuthorizationServiceProvider>();
        // Note: Do not change this kind of service resolution, as it must be done on a per-scope basis (e.g. see TbxController)!
        services.AddScoped<IAuthorizationService>(
            sp => sp.GetRequiredService<OverrideableAuthorizationServiceProvider>());

        services.AddScoped<IdentityService>();
        services.AddScoped<OverrideableIdentityServiceProvider>();
        // Note: Do not change this kind of service resolution, as it must be done on a per-scope basis (e.g. see TbxController)!
        services.AddScoped<IIdentityService>(
            sp => sp.GetRequiredService<OverrideableIdentityServiceProvider>());

        services.AddScoped<ILanguageRepository, LanguageRepository>();
        services.AddScoped<IOfficeRepository, OfficeRepository>();
        services.AddScoped<ILenochClassificationRepository, LenochClassificationRepository>();
        services.AddScoped<ICollectionRepository, CollectionRepository>();
        services.AddScoped<IClassificationRepository, ClassificationRepository>();
        services.AddScoped<IUserAccountRepository, UserAccountRepository>();
        services.AddScoped<IAdminInfoRepository, AdminInfoRepository>();
        services.AddScoped<IEntryRepository, EntryRepository>();
        services.AddScoped<IConceptSystemPositionRepository, ConceptSystemPositionRepository>();
        services.AddScoped<IPictureRepository, PictureRepository>();
        services.AddScoped<IGlossaryRepository, GlossaryRepository>();
        services.AddScoped<ISubjectRepository, SubjectRepository>();
        services.AddScoped<IDomainRepository, DomainRepository>();
        services.AddScoped<ILenochClassificationSubjectDomainMappingRepository, LenochClassificationSubjectDomainMappingRepository>();
        services.AddScoped<IStatisticRepository, StatisticRepository>();
        services.AddScoped<IEntryTemplateRepository, EntryTemplateRepository>();
        services.AddScoped<ISourceTemplateRepository, SourceTemplateRepository>();
        services.AddScoped<INoteSubCodeRepository, NoteSubCodeRepository>();
        services.AddScoped<IDuplicateCheckRepository, DuplicateCheckRepository>();
        services.AddScoped<INewsRepository, NewsRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddScoped<ISearchApiClient, SearchApiClient>();
        services.AddScoped<ISearchQueueClient, SearchQueueClient>();

        services.AddHostedService<EntryUnlockBackgroundService>();
        services.AddHostedService<StatisticDeleteBackgroundService>();
        services.AddHostedService<QueuedBackgroundService>();

        // If there is more demand, capacity should be increased.
        services.AddSingleton<IBackgroundTaskQueue>(_ => new BackgroundTaskQueue(20));
    }
}
