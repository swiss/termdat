using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Bk.Termdat.Business.Resource.Validators;
using Bk.Termdat.Common.Api;
using Bk.Termdat.Common.Api.Middleware;
using Bk.Termdat.Common.Api.ModelBinder;
using Bk.Termdat.CrossCutting.Configuration;
using Bk.Termdat.Infrastructure.DataSource;
using Bk.Termdat.Infrastructure.Service.RabbitMq;
using Bk.Termdat.Search.Api;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Swiss.FCh.Monitoring.Extensions;
using Swiss.FCh.Utils.Converter;
using Swiss.FCh.Utils.Extensions;
using Swiss.FCh.Utils.Rhos.Extensions;

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder
        .AddSerilog("Bk.TERMDAT.Search.Api")
        .AddRhosConfigurations()
        .AddRhosPostgresConfiguration(vaultPath: "/app/vault/pg-database-credentials.json")
        .AddRhosRabbitMqConfiguration(vaultPath: "/app/vault/rabbitmq-credentials.json");

    builder.Services.AddValidatedOptions<SearchOptions>(builder.Configuration, SearchOptions.SectionKey);
    var authenticationOptionsSection = builder.Services.AddValidatedOptions<AuthenticationOptions>(builder.Configuration, AuthenticationOptions.SectionKey);
    var authorizationOptionsSection = builder.Services.AddValidatedOptions<AuthorizationOptions>(builder.Configuration, AuthorizationOptions.SectionKey);
    var swaggerOptionsSection = builder.Services.AddValidatedOptions<SwaggerOptions>(builder.Configuration, SwaggerOptions.SectionKey);

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

    builder.Services.AddTermdat();

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

    builder
        .AddRabbitMq()
        .AddRabbitMqConsumer();

    builder.Services
        .AddHealthChecks()
        .AddDatabase<DataContext>()
        .AddCheck<RabbitMqHealthCheck>("rabbitMq");

    var app = builder.Build();

    if (app.Environment.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
    }

    if (swaggerOptions.Enabled.GetValueOrDefault(false))
    {
        app.UseSwaggerWithUiAndOAuth(authenticationOptions.ClientId, "TERMDAT 3.0 - Search API");
    }

    app.UseMiddleware<ExceptionMiddleware>(app.Environment.IsDevelopment());
    app.UseSerilogRequestLogging();

    app.UseAuthentication();
    app.UseRouting();
    app.UseAuthorization();

    app.MapControllers();
    app.MapFChHealthChecks();

    Log.Information("Starting web host: Bk.Termdat.Search.Api");
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
