using System.Threading.Tasks;
using RabbitMQ.Client;

namespace Bk.Termdat.Infrastructure.Service.RabbitMq;

public class RabbitMqProducerChannelFactory : IRabbitMqChannelFactory
{
    private readonly IConnection connection;

    public RabbitMqProducerChannelFactory(IConnectionFactory connectionFactory)
    {
        connection = connectionFactory.CreateConnectionAsync().Result;

        using var tempChannel = connection.CreateChannelAsync().Result;

        // Create exchange
        tempChannel.ExchangeDeclareAsync("termdat.search", ExchangeType.Fanout, true).Wait();
    }

    public string GetQueueName()
    {
        return string.Empty;
    }

    public async Task<IChannel> CreateChannel()
    {
        var channel = await connection.CreateChannelAsync();
        return channel;
    }
}
