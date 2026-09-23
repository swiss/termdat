using System.Net;
using Bk.Monitoring.Extensions;
using Bk.Sparql.Extensions;
using Bk.Termdat.CrossCutting.Configuration;
using Bk.Termdat.Infrastructure.DataSource;
using Bk.Termdat.LindasExport.Api;
using Bk.Termdat.LindasExport.Api.Configuration;
using Bk.Utils.Extensions;
using Bk.Utils.Rhos.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Serilog;

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder
        .AddSerilog("Bk.TERMDAT.LindasExport.Api")
        .AddRhosConfigurations()
        .AddRhosPostgresConfiguration(vaultPath: "/app/vault/pg-database-credentials.json");

    var sparqlTargetsOptions = builder.Services.AddValidatedOptions<SparqlTargetsOptions>(builder.Configuration, SparqlTargetsOptions.SectionKey).Get<SparqlTargetsOptions>()!;
    builder.Services.AddValidatedOptions<TermdatGraphOptions>(builder.Configuration, TermdatGraphOptions.SectionKey);
    builder.Services.AddValidatedOptions<LindasExportOptions>(builder.Configuration, LindasExportOptions.SectionKey);
    var swaggerOptions = builder.Services.AddValidatedOptions<SwaggerOptions>(builder.Configuration, SwaggerOptions.SectionKey).Get<SwaggerOptions>()!;

    builder.Services.AddDbContext<DataContext>(o =>
    {
        if (builder.Environment.IsDevelopment())
        {
            o.ConfigureWarnings(w => w.Throw(RelationalEventId.MultipleCollectionIncludeWarning));
            o.EnableSensitiveDataLogging();
        }

        var connectionString = builder.Configuration.GetPostgresConnectionString();
        o.UseNpgsql(connectionString,
            o => { o.MigrationsHistoryTable("__EFMigrationsHistory", DataContext.Schema).EnableRetryOnFailure(1); });
    });

    builder.Services.AddHttpClient();

    // Register HTTP clients for each SPARQL target
    foreach (var target in sparqlTargetsOptions.Targets.OrderBy(t => t.Key))
    {
        builder.Services.AddHttpClient(target.Key)
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {
                Proxy = target.Value.Proxy.UseProxy ? new WebProxy(target.Value.Proxy.Address, false) : null,
                UseProxy = target.Value.Proxy.UseProxy
            });
    }

    builder.Services.AddControllers();

    builder.Services.AddLindasExport();

    builder.Services.AddSparqlClient();

    if (swaggerOptions is { Enabled: true })
    {
        builder.Services.AddSwaggerGen(c =>
        {
            c.CustomOperationIds(e =>
                $"{e.ActionDescriptor.RouteValues["controller"]}_{e.RelativePath}_{e.HttpMethod}");
        });
    }

    var healthChecksBuilder = builder.Services.AddHealthChecks();

    // Add health checks for each SPARQL target
    foreach (var target in sparqlTargetsOptions.Targets.OrderBy(t => t.Key))
    {
        healthChecksBuilder.AddUrl(
            $"SPARQL Target - {target.Key}",
            target.Value.HealthEndPoint,
            HealthStatus.Degraded,
            target.Value.Proxy.UseProxy ? target.Key : null
        );
    }

    var app = builder.Build();

    if (app.Environment.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
    }

    if (swaggerOptions is { Enabled: true })
    {
        app.UseSwagger(config =>
        {
            config.RouteTemplate = "api/swagger/{documentName}/swagger.{json|yaml}";
        });
        app.UseSwaggerUI(config =>
        {
            config.SwaggerEndpoint("/api/swagger/v1/swagger.json", "TERMDAT 3.0 - Lindas Export API");
            config.RoutePrefix = "api/swagger";
        });
    }

    app.UseSerilogRequestLogging();

    app.UseRouting();

    app.MapControllers();
    app.MapBkHealthChecks();

    Log.Information("Starting web host: Bk.Termdat.LindasExport.Api");
    app.Run();

    return 0;
}
// For more info, see: https://github.com/dotnet/runtime/issues/60600#issuecomment-1068323222
catch (Exception ex) when (ex is not HostAbortedException)
{
    Log.Fatal(ex, "Host terminated unexpectedly");
    return 1;
}
finally
{
    Log.CloseAndFlush();
}
