using Bk.Termdat.Business.Domain;
using Bk.Termdat.Business.Repositories;
using Bk.Termdat.Business.Services;
using Bk.Termdat.Business.Services.Interfaces;
using Bk.Termdat.Infrastructure.DataSource;
using Bk.Termdat.Infrastructure.DataSource.Repositories;
using Bk.Termdat.Infrastructure.Search;
using Bk.Termdat.Infrastructure.Search.Repositories;
using Bk.Termdat.Infrastructure.Service;
using Bk.Termdat.Search.Api.BackgroundServices;
using Microsoft.Extensions.DependencyInjection;

namespace Bk.Termdat.Search.Api;

internal static class ServiceCollectionExtension
{
    internal static void AddTermdat(this IServiceCollection services)
    {
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<ISearchIndexService, SearchIndexService>();
        services.AddScoped<SearchIndexEntryEventConsumer>();

        services.AddScoped<IAuthorizationService, AuthorizationService>();

        services.AddSingleton<ISearchGateway, SearchGateway>();
        services.AddScoped<ISearchRepository, SearchRepository>();
        services.AddScoped<IEntryRepository, EntryRepository>();
        services.AddScoped<IUserAccountRepository, UserAccountRepository>();

        services.AddHostedService<CreateIndexBackgroundService>();
        services.AddHostedService<SearchRefreshBackgroundService>();
        services.AddHostedService<MessageQueueBackgroundService>();
    }
}
