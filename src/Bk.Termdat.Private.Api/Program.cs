using System;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using Bk.Termdat.Business.Mapping.Profiles;
using Bk.Termdat.Business.Resource.Validators;
using Bk.Termdat.Common.Api;
using Bk.Termdat.Common.Api.Middleware;
using Bk.Termdat.Common.Api.ModelBinder;
using Bk.Termdat.CrossCutting.Configuration;
using Bk.Termdat.Infrastructure.DataSource;
using Bk.Termdat.Infrastructure.Service;
using Bk.Termdat.Infrastructure.Service.RabbitMq;
using Bk.Termdat.Private.Api;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Swiss.FCh.DocumentService.Client.Extensions;
using Swiss.FCh.Monitoring.Extensions;
using Swiss.FCh.Utils.Converter;
using Swiss.FCh.Utils.Extensions;
using Swiss.FCh.Utils.Rhos.Extensions;

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder
        .AddSerilog("Bk.TERMDAT.Private.Api")
        .AddRhosConfigurations()
        .AddRhosPostgresConfiguration(vaultPath: "/app/vault/pg-database-credentials.json")
        .AddRhosRabbitMqConfiguration(vaultPath: "/app/vault/rabbitmq-credentials.json");

    builder.Services.AddValidatedOptions<SearchQueueOptions>(builder.Configuration, SearchQueueOptions.SectionKey);
    builder.Services.AddValidatedOptions<PrivateOptions>(builder.Configuration, PrivateOptions.SectionKey);
    builder.Services.AddValidatedOptions<EntryNotePatternOptions>(builder.Configuration, EntryNotePatternOptions.SectionKey);
    builder.Services.AddValidatedOptions<TermdatClientOptions>(builder.Configuration, TermdatClientOptions.SectionKey);
    builder.Services.AddValidatedOptions<FeedbackOptions>(builder.Configuration, FeedbackOptions.SectionKey);
    builder.Services.AddValidatedOptions<FileImportOptions>(builder.Configuration, FileImportOptions.SectionKey);
    builder.Services.AddValidatedOptions<EntryUnlockOptions>(builder.Configuration, EntryUnlockOptions.SectionKey);
    builder.Services.AddValidatedOptions<StatisticDeleteOptions>(builder.Configuration, StatisticDeleteOptions.SectionKey);
    builder.Services.AddValidatedOptions<EmailOptions>(builder.Configuration, EmailOptions.SectionKey);
    builder.Services.AddValidatedOptions<FrontendOptions>(builder.Configuration, FrontendOptions.SectionKey);
    var migrationOptions = builder.Services.AddValidatedOptions<MigrationOptions>(builder.Configuration, MigrationOptions.SectionKey).Get<MigrationOptions>()!;
    var searchApiOptionsSection = builder.Services.AddValidatedOptions<SearchApiOptions>(builder.Configuration, SearchApiOptions.SectionKey);
    var authenticationOptionsSection = builder.Services.AddValidatedOptions<AuthenticationOptions>(builder.Configuration, AuthenticationOptions.SectionKey);
    var authorizationOptionsSection = builder.Services.AddValidatedOptions<AuthorizationOptions>(builder.Configuration, AuthorizationOptions.SectionKey);
    var swaggerOptionsSection = builder.Services.AddValidatedOptions<SwaggerOptions>(builder.Configuration, SwaggerOptions.SectionKey);
    var autoMapperOptions = builder.Services.AddValidatedOptions<AutoMapperOptions>(builder.Configuration, AutoMapperOptions.SectionKey).Get<AutoMapperOptions>();

    builder.Services.AddHttpContextAccessor();
    builder.Services
        .AddControllers(options =>
        {
            options.ModelBinderProviders.Insert(0, new StringSanitizerModelBinderProvider());
        })
        .AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
            options.JsonSerializerOptions.Converters.Add(new SanitizeStringJsonConverter());
        });

    builder.Services.AddDbContext<DataContext>(o =>
    {
        if (builder.Environment.IsDevelopment())
        {
            o.ConfigureWarnings(w => w.Throw(RelationalEventId.MultipleCollectionIncludeWarning));
            o.EnableSensitiveDataLogging();
        }

        var connectionString = builder.Configuration.GetPostgresConnectionString();
        o.UseNpgsql(connectionString, ob => { ob.MigrationsHistoryTable("__EFMigrationsHistory", schema: DataContext.Schema).EnableRetryOnFailure(1); });
    });

    builder.Services.AddEmailService(builder.Configuration);
    builder.Services.AddDocumentService(builder.Configuration);
    builder.Services.AddTermdat();

    builder.Services.AddAutoMapper((_, cfg) => cfg.LicenseKey = autoMapperOptions.LicenseKey, typeof(SharedMappingProfile));

    var authenticationOptions = authenticationOptionsSection.Get<AuthenticationOptions>();

    var swaggerOptions = swaggerOptionsSection.Get<SwaggerOptions>();
    if (swaggerOptions.Enabled.GetValueOrDefault(false))
    {
        builder.Services.AddSwaggerGenWithKeyCloak(authenticationOptions);
    }

    builder.Services.AddFluentValidationAutoValidation();
    builder.Services.AddFluentValidationClientsideAdapters();
    builder.Services.AddValidatorsFromAssemblyContaining<RecommendQueryValidator>();

    builder.Services.AddAuthenticationWithJwtBearer(authenticationOptions);
    var authorizationOptions = authorizationOptionsSection.Get<AuthorizationOptions>();
    builder.Services.AddTermdatAuthorizationPolicies(authorizationOptions);

    var searchApiOptions = searchApiOptionsSection.Get<SearchApiOptions>();
    builder.Services.AddHttpClient(SearchApiOptions.HttpClientName, client =>
    {
        client.BaseAddress = new Uri(searchApiOptions.BaseUrl);
    }).ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler()
    {
        AllowAutoRedirect = false,
        UseProxy = false
    });

    builder
        .AddRabbitMq()
        .AddRabbitMqProducer();

    builder.Services
        .AddMemoryCache()
        .AddHealthChecks()
        .AddDatabase<DataContext>()
        .AddCheck<SearchApiHealthCheck>("searchService")
        .AddCheck<RabbitMqHealthCheck>("rabbitMq")
        .AddDocumentService();

    var app = builder.Build();

    Log.Information("Database auto migration is set to: {Value}", migrationOptions.IsAutoMigration);
    if (migrationOptions.IsAutoMigration)
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<DataContext>();
        db.Database.Migrate();
    }

    if (app.Environment.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
    }

    if (swaggerOptions.Enabled.GetValueOrDefault(false))
    {
        app.UseSwaggerWithUiAndOAuth(authenticationOptions.ClientId, "TERMDAT 3.0 - Private API");
    }

    var supportedCultures = new[] { "de", "en", "it", "fr", "rm" };

    var localizationOptions = new RequestLocalizationOptions()
        .SetDefaultCulture(supportedCultures[0])
        .AddSupportedCultures(supportedCultures)
        .AddSupportedUICultures(supportedCultures)
        .AddInitialRequestCultureProvider(new AcceptLanguageHeaderRequestCultureProvider());
    app.UseRequestLocalization(localizationOptions);

    app.UseMiddleware<ExceptionMiddleware>(app.Environment.IsDevelopment());
    app.UseSerilogRequestLogging();

    app.UseAuthentication();
    app.UseRouting();
    app.UseAuthorization();

    app.MapControllers();
    app.MapFChHealthChecks();

    Log.Information("Starting web host: Bk.Termdat.Private.Api");
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
