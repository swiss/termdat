using System.Threading.Tasks;
using RabbitMQ.Client;

namespace Bk.Termdat.Infrastructure.Service.RabbitMq;

public interface IRabbitMqChannelFactory
{
    string GetQueueName();
    Task<IChannel> CreateChannel();
}
