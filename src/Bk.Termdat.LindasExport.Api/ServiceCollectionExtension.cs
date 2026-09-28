using Bk.Termdat.Business.Repositories;
using Bk.Termdat.Infrastructure.DataSource.Repositories;
using Bk.Termdat.LindasExport.Api.BackgroundServices;
using Bk.Termdat.LindasExport.Api.Interfaces;
using Bk.Termdat.LindasExport.Api.Mapping;
using Bk.Termdat.LindasExport.Api.Services;

namespace Bk.Termdat.LindasExport.Api;

internal static class ServiceCollectionExtension
{
    internal static void AddLindasExport(this IServiceCollection services)
    {
        services.AddTransient<ITermdatGraphService, TermdatGraphService>();
        services.AddTransient<ISparqlClientFactory, SparqlClientFactory>();
        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<IExportScheduler, ExportScheduler>();
        services.AddSingleton<ILindasOperationExecutor, LindasOperationExecutor>();
        services.AddSingleton<IChunkDelay, ChunkDelay>();

        services.AddScoped<ILindasMapper, LindasMapper>();

        services.AddScoped<ICollectionRepository, CollectionRepository>();

        services.AddScoped<ILindasExportService, LindasExportService>();

        services.AddHostedService<LindasExportBackgroundService>();
    }
}
