using System;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Text.Json.Serialization;
using Bk.Termdat.Business.Public.Api.Mapping;
using Bk.Termdat.Common.Api.Middleware;
using Bk.Termdat.Common.Api.ModelBinder;
using Bk.Termdat.CrossCutting.Configuration;
using Bk.Termdat.Infrastructure.DataSource;
using Bk.Termdat.Infrastructure.Service;
using Bk.Termdat.Public.Api;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi;
using Serilog;
using Swashbuckle.AspNetCore.Filters;
using Swiss.FCh.Monitoring.Extensions;
using Swiss.FCh.Utils.Converter;
using Swiss.FCh.Utils.Extensions;
using Swiss.FCh.Utils.Rhos.Extensions;

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder
        .AddSerilog("Bk.TERMDAT.Public.Api")
        .AddRhosConfigurations()
        .AddRhosPostgresConfiguration(vaultPath: "/app/vault/pg-database-credentials.json");

    var searchApiOptionsSection = builder.Services.AddValidatedOptions<SearchApiOptions>(builder.Configuration, SearchApiOptions.SectionKey);
    builder.Services.AddValidatedOptions<PublicApiOptions>(builder.Configuration, PublicApiOptions.SectionKey);
    builder.Services.AddValidatedOptions<TermdatClientOptions>(builder.Configuration, TermdatClientOptions.SectionKey);
    builder.Services.AddValidatedOptions<AnalyticsOptions>(builder.Configuration, AnalyticsOptions.SectionKey);
    var autoMapperOptions = builder.Services.AddValidatedOptions<AutoMapperOptions>(builder.Configuration, AutoMapperOptions.SectionKey).Get<AutoMapperOptions>();

    builder.Services.AddHttpContextAccessor();
    builder.Services
        .AddControllers(options =>
        {
            options.ModelBinderProviders.Insert(0, new StringSanitizerModelBinderProvider());
        })
        .AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault;
            options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            options.JsonSerializerOptions.Converters.Add(new SanitizeStringJsonConverter());
        });

    builder.Services.AddDbContext<DataContext>(dbContextOptions =>
    {
        if (builder.Environment.IsDevelopment())
        {
            dbContextOptions.ConfigureWarnings(w => w.Throw(RelationalEventId.MultipleCollectionIncludeWarning));
            dbContextOptions.EnableSensitiveDataLogging();
        }

        var connectionString = builder.Configuration.GetPostgresConnectionString();
        dbContextOptions.UseNpgsql(connectionString, pgSqlOptions => { pgSqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", schema: DataContext.Schema).EnableRetryOnFailure(1); });
    });

    builder.Services.AddTermdat();

    builder.Services.AddAutoMapper((_, cfg) => cfg.LicenseKey = autoMapperOptions.LicenseKey, typeof(PublicApiMappingProfile));

    builder.Services.AddApiVersioning(opt =>
    {
        opt.ReportApiVersions = true;
        opt.DefaultApiVersion = new ApiVersion(2, 0);
    });

    builder.Services.AddSwaggerGen(config =>
    {
        config.SwaggerDoc("v2", new OpenApiInfo { Title = "TERMDAT 3.0 - Public API", Version = "v2" });
        var apiXmlDocumentationPath = Path.Combine(AppContext.BaseDirectory, "Bk.Termdat.Public.Api.xml");
        config.IncludeXmlComments(apiXmlDocumentationPath);
        var apiBusinessXmlDocumentationPath = Path.Combine(AppContext.BaseDirectory, "Bk.Termdat.Business.Public.Api.xml");
        config.IncludeXmlComments(apiBusinessXmlDocumentationPath);
        config.ExampleFilters();
    });

    builder.Services.AddSwaggerExamplesFromAssemblies(Assembly.GetEntryAssembly());

    var searchApiOptions = searchApiOptionsSection.Get<SearchApiOptions>();
    builder.Services.AddHttpClient(SearchApiOptions.HttpClientName, client =>
    {
        client.BaseAddress = new Uri(searchApiOptions.BaseUrl);
    }).ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler()
    {
        AllowAutoRedirect = false,
        UseProxy = false
    });

    builder.Services
        .AddHealthChecks()
        .AddDatabase<DataContext>()
        .AddCheck<SearchApiHealthCheck>("searchService");

    var app = builder.Build();

    if (app.Environment.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
    }

    app.UseSwagger();
    app.UseSwaggerUI(config =>
    {
        config.SwaggerEndpoint($"/swagger/v2/swagger.json", "TERMDAT 3.0 - Public API");
    });

    app.UseMiddleware<ExceptionMiddleware>(app.Environment.IsDevelopment());
    app.UseSerilogRequestLogging();

    app.UseRouting();

    app.MapControllers();
    app.MapFChHealthChecks("/v2/systemstatus");

    Log.Information("Starting web host: Bk.Termdat.Public.Api");
    await app.RunAsync();

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
    await Log.CloseAndFlushAsync();
}
