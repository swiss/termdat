using System;
using System.Threading.Tasks;
using RabbitMQ.Client;

namespace Bk.Termdat.Infrastructure.Service.RabbitMq;

public class RabbitMqConsumerChannelFactory : IRabbitMqChannelFactory
{
    private readonly IConnection connection;

    public RabbitMqConsumerChannelFactory(IConnectionFactory connectionFactory)
    {
        ArgumentNullException.ThrowIfNull(connectionFactory);

        connection = connectionFactory.CreateConnectionAsync().Result;

        using var tempChannel = connection.CreateChannelAsync().Result;

        // Create queue
        var queueName = GetQueueName();
        tempChannel.QueueDeclareAsync(queueName, durable: true, exclusive: false, autoDelete: false).Wait();

        // Create and bind exchange
        tempChannel.ExchangeDeclareAsync("termdat.search", ExchangeType.Fanout, true).Wait();
        tempChannel.QueueBindAsync(queueName, "termdat.search", string.Empty);
    }

    public string GetQueueName()
    {
        var queueName = "entry-search-index";
        var hostname = Environment.GetEnvironmentVariable("HOSTNAME");
        if (!string.IsNullOrWhiteSpace(hostname))
        {
            queueName += $"-{hostname}";
        }

        return queueName;
    }

    public async Task<IChannel> CreateChannel()
    {
        var channel = await connection.CreateChannelAsync();
        return channel;
    }
}
