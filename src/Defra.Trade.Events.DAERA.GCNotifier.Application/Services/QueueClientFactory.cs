// Copyright DEFRA (c). All rights reserved.
// Licensed under the Open Government License v3.0.

using System.Collections.Concurrent;
using Azure.Messaging.ServiceBus;
using Defra.Trade.Events.DAERA.GCNotifier.Application.Models;
using Defra.Trade.Events.DAERA.GCNotifier.Application.Services.Contracts;
using Microsoft.Extensions.Options;

namespace Defra.Trade.Events.DAERA.GCNotifier.Application.Services;

public class QueueClientFactory(IOptions<GcNotificationSubscriberSettings> settings, ServiceBusClient client) : IQueueClientFactory, IAsyncDisposable
{
    private readonly IOptions<GcNotificationSubscriberSettings> _settings = settings ?? throw new ArgumentNullException(nameof(settings));
    private readonly ServiceBusClient _client = client ?? throw new ArgumentNullException(nameof(client));
    private readonly ConcurrentDictionary<string, ServiceBusSender> _senders = new();

    public ServiceBusSender CreateNotifierQueueClient()
    {
        var queueName = _settings.Value.RemosGCNotifierQueue;
        return _senders.GetOrAdd(queueName, key => _client.CreateSender(key));
    }

    public async ValueTask DisposeAsync()
    {
        foreach (var sender in _senders.Values)
        {
            await sender.DisposeAsync();
        }
        GC.SuppressFinalize(this);
    }
}
