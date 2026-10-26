using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Bk.Termdat.Business.Clients;
using Bk.Termdat.Business.Domain.Projections;
using Bk.Termdat.Business.Messages;
using Bk.Termdat.CrossCutting.Configuration;
using Bk.Termdat.Infrastructure.Service.RabbitMq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace Bk.Termdat.Infrastructure.Service;

public class SearchQueueClient : ISearchQueueClient, IDisposable
{
    private readonly IChannel channel;
    private readonly SearchQueueOptions searchQueueOptions;
    private readonly ILogger<SearchQueueClient> logger;

    public SearchQueueClient(IRabbitMqChannelFactory channelFactory, IOptions<SearchQueueOptions> searchQueueOptions, ILogger<SearchQueueClient> logger)
    {
        ArgumentNullException.ThrowIfNull(channelFactory);
        ArgumentNullException.ThrowIfNull(searchQueueOptions);

        channel = channelFactory.CreateChannel().Result;

        this.searchQueueOptions = searchQueueOptions.Value;
        this.logger = logger;
    }

    public async Task QueueIndexAdd(IEnumerable<IndexEntry> indexEntries)
    {
        await PublishMessage(new SearchIndexEntryAddEvent { IndexEntries = indexEntries }, "search.index.add");
    }

    public async Task QueueIndexUpdate(IEnumerable<IndexEntry> indexEntries)
    {
        await PublishMessage(new SearchIndexEntryUpdateEvent { IndexEntries = indexEntries }, "search.index.update");
    }

    public async Task QueueIndexUpdateByIds(IEnumerable<int> ids)
    {
        await PublishMessage(new SearchIndexEntryUpdateByEntryIdsEvent { EntryIds = ids }, "search.index.update.byids");
    }

    public async Task QueueIndexDelete(IEnumerable<int> entryIds)
    {
        await PublishMessage(new SearchIndexEntryDeleteEvent { EntryIds = entryIds }, "search.index.delete");
    }

    private async Task PublishMessage<T>(T message, string routingKey) where T : class
    {
        try
        {
            using var source = new CancellationTokenSource(TimeSpan.FromSeconds(searchQueueOptions.PublishTimeoutInSeconds));
            var json = JsonSerializer.Serialize(message);
            var body = Encoding.UTF8.GetBytes(json);
            await channel.BasicPublishAsync("termdat.search", routingKey, false, new BasicProperties(), body, source.Token);
        }
        catch (OperationCanceledException e)
        {
            if (logger.IsEnabled(LogLevel.Critical))
            {
                logger.LogCritical(e,
                    "Could not send {Name} because operation was canceled after {PublishTimeoutInSeconds} seconds. Message: {Message}",
                    typeof(T).Name, searchQueueOptions.PublishTimeoutInSeconds, e.Message);
            }
        }
        catch (Exception e)
        {
            if (logger.IsEnabled(LogLevel.Critical))
            {
                logger.LogCritical(e,
                "Could not send {Name} because no connection could be established. Message: {Message}", typeof(T).Name, e.Message);
            }
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            channel?.Dispose();
        }
    }
}
