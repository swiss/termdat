using System;
using System.IO;
using Bk.Termdat.Infrastructure.Service.RabbitMq;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using Serilog;
using Swiss.FCh.Utils.Rhos;

namespace Bk.Termdat.Common.Api;

public static class ApplicationBuilderExtensions
{
    public static void UseSwaggerWithUiAndOAuth(this IApplicationBuilder app, string clientId, string endpointName)
    {
        app.UseSwagger(config =>
        {
            config.RouteTemplate = "api/swagger/{documentName}/swagger.{json|yaml}";
        });
        app.UseSwaggerUI(config =>
        {
            config.SwaggerEndpoint("/api/swagger/v1/swagger.json", endpointName);
            config.RoutePrefix = "api/swagger";

            config.OAuthClientId(clientId);
            config.OAuthScopes("openid");
            config.OAuthUsePkce();
        });
    }

    public static IHostApplicationBuilder AddRhosRabbitMqConfiguration(this IHostApplicationBuilder builder, string stageEnvVarName = "STAGE", string vaultPath = "../vault/secrets/rabbitmq-credentials.json")
    {
        var stage = Environment.GetEnvironmentVariable(stageEnvVarName);
        if (Stage.IsRhosStage(stage))
        {
            Log.Information("RHOS stage={Stage} detected, adding rabbitmq vault secret files", stage);

            if (vaultPath.StartsWith("..")) //secrets files are outside the applications scope
            {
                // These files are placed in the pod by the vault agent, and they match the secrets of our appsettings.json file
                // ConfigurationBuilder.AddJsonFile will NOT load files that are above the root directory, hence the stream reading...
                builder.Configuration.AddJsonStream(File.OpenRead(vaultPath));
            }
            else
            {
                builder.Configuration.AddJsonFile(vaultPath); //files inside the applications scope can be loaded "normally"
            }
        }

        return builder;
    }

    public static WebApplicationBuilder AddRabbitMq(this WebApplicationBuilder builder)
    {
        builder.Services.AddSingleton<IConnectionFactory>(new ConnectionFactory
        {
            HostName = builder.Configuration["rabbitmq:clusterUrl"]!,
            Port = int.Parse(builder.Configuration["rabbitmq:clusterPort"]!),
            UserName = builder.Configuration["rabbitmq:default_user"]!,
            Password = builder.Configuration["rabbitmq:default_pass"]!,
            AutomaticRecoveryEnabled = true,
            NetworkRecoveryInterval = TimeSpan.FromSeconds(10),
            TopologyRecoveryEnabled = true,
        });

        return builder;
    }

    public static WebApplicationBuilder AddRabbitMqProducer(this WebApplicationBuilder builder)
    {
        builder.Services.AddSingleton<IRabbitMqChannelFactory, RabbitMqProducerChannelFactory>();
        return builder;
    }

    public static WebApplicationBuilder AddRabbitMqConsumer(this WebApplicationBuilder builder)
    {
        builder.Services.AddSingleton<IRabbitMqChannelFactory, RabbitMqConsumerChannelFactory>();
        return builder;
    }
}
