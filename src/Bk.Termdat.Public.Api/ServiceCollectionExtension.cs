using Bk.Termdat.Business.Clients;
using Bk.Termdat.Business.Public.Api.Services;
using Bk.Termdat.Business.Repositories;
using Bk.Termdat.Business.Services;
using Bk.Termdat.Business.Services.Interfaces;
using Bk.Termdat.Infrastructure.DataSource.Repositories;
using Bk.Termdat.Infrastructure.Service;
using Microsoft.Extensions.DependencyInjection;
using ClassificationService = Bk.Termdat.Business.Public.Api.Services.ClassificationService;
using CollectionService = Bk.Termdat.Business.Public.Api.Services.CollectionService;
using EntryService = Bk.Termdat.Business.Public.Api.Services.EntryService;
using IClassificationService = Bk.Termdat.Business.Public.Api.Services.IClassificationService;
using ICollectionService = Bk.Termdat.Business.Public.Api.Services.ICollectionService;
using IEntryService = Bk.Termdat.Business.Public.Api.Services.IEntryService;

namespace Bk.Termdat.Public.Api;

internal static class ServiceCollectionExtension
{
    internal static void AddTermdat(this IServiceCollection services)
    {
        services.AddScoped<IClassificationService, ClassificationService>();
        services.AddScoped<ICollectionService, CollectionService>();
        services.AddScoped<ICollectionRepository, CollectionRepository>();
        services.AddScoped<IEntryService, EntryService>();
        services.AddScoped<ISearchService, SearchService>();
        services.AddScoped<ITermdatUrlService, TermdatUrlService>();
        services.AddScoped<IBackendTextHelperService, BackendTextHelperService>();
        services.AddScoped<ILanguageValidationService, LanguageValidationService>();
        services.AddScoped<IAnalyticsService, AnalyticsService>();

        services.AddScoped<IClassificationRepository, ClassificationRepository>();
        services.AddScoped<IEntryRepository, EntryRepository>();
        services.AddScoped<ICollectionRepository, CollectionRepository>();
        services.AddScoped<IClassificationRepository, ClassificationRepository>();
        services.AddScoped<ILanguageRepository, LanguageRepository>();

        services.AddScoped<ISearchApiClient, SearchApiClient>();
    }
}
