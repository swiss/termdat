using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Bk.Termdat.Business.Messages;
using Bk.Termdat.Infrastructure.Service;
using Bk.Termdat.Infrastructure.Service.RabbitMq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Bk.Termdat.Search.Api.BackgroundServices;

public class MessageQueueBackgroundService : BackgroundService
{
    private readonly string queueName;
    private readonly IChannel channel;
    private readonly IServiceScopeFactory scopeFactory;

    public MessageQueueBackgroundService(IRabbitMqChannelFactory channelFactory, IServiceScopeFactory scopeFactory)
    {
        queueName = channelFactory.GetQueueName();
        channel = channelFactory.CreateChannel().Result;
        this.scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += async (_, ea) =>
        {
            using var scope = scopeFactory.CreateScope();
            var consumerService = scope.ServiceProvider.GetRequiredService<SearchIndexEntryEventConsumer>();

            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            var routingKey = ea.RoutingKey;

            if (routingKey == "search.index.add")
            {
                var data = JsonSerializer.Deserialize<SearchIndexEntryAddEvent>(message)!;
                await consumerService.Consume(data);
            }

            if (routingKey == "search.index.update")
            {
                var data = JsonSerializer.Deserialize<SearchIndexEntryUpdateEvent>(message)!;
                await consumerService.Consume(data);
            }

            if (routingKey == "search.index.update.byids")
            {
                var data = JsonSerializer.Deserialize<SearchIndexEntryUpdateByEntryIdsEvent>(message)!;
                await consumerService.Consume(data);
            }

            if (routingKey == "search.index.delete")
            {
                var data = JsonSerializer.Deserialize<SearchIndexEntryDeleteEvent>(message)!;
                await consumerService.Consume(data);
            }

            await channel.BasicAckAsync(ea.DeliveryTag, multiple: false, stoppingToken);
        };

        await channel.BasicConsumeAsync(queueName, false, consumer, stoppingToken);
    }
}
