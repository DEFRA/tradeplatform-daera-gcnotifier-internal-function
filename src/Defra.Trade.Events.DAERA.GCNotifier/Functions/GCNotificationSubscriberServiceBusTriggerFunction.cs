// Copyright DEFRA (c). All rights reserved.
// Licensed under the Open Government License v3.0.

using System;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Defra.Trade.Common.Functions.Isolated;
using Defra.Trade.Common.Functions.Isolated.Extensions;
using Defra.Trade.Common.Functions.Isolated.Interfaces;
using Defra.Trade.Events.DAERA.GCNotifier.Application.Dtos.Inbound;
using Defra.Trade.Events.DAERA.GCNotifier.Application.Models;
using Defra.Trade.Events.DAERA.GCNotifier.Application.Services.Contracts;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Defra.Trade.Events.DAERA.GCNotifier.Functions;

public sealed class GcNotificationSubscriberServiceBusTriggerFunction
{
    private readonly IBaseMessageProcessorService<GCNotificationInbound> _baseMessageProcessorService;
    private readonly IMessageRetryService _retry;
    private readonly IQueueClientFactory _queueClientFactory;
    private readonly ILogger<GcNotificationSubscriberServiceBusTriggerFunction> _logger;

    public GcNotificationSubscriberServiceBusTriggerFunction(
        IBaseMessageProcessorService<GCNotificationInbound> baseMessageProcessorService,
        IMessageRetryService retry,
        IQueueClientFactory queueClientFactory,
        ILogger<GcNotificationSubscriberServiceBusTriggerFunction> logger)
    {
        ArgumentNullException.ThrowIfNull(baseMessageProcessorService);
        ArgumentNullException.ThrowIfNull(retry);
        ArgumentNullException.ThrowIfNull(queueClientFactory);
        ArgumentNullException.ThrowIfNull(logger);
        _baseMessageProcessorService = baseMessageProcessorService;
        _retry = retry;
        _queueClientFactory = queueClientFactory;
        _logger = logger;
    }

    [Function(nameof(GcNotificationSubscriberServiceBusTriggerFunction))]
    public async Task RunAsync(
        [ServiceBusTrigger(GcNotificationSubscriberSettings.DefaultQueueName, Connection = GcNotificationSubscriberSettings.ConnectionStringConfigurationKey, IsSessionsEnabled = false)] ServiceBusReceivedMessage message,
        ServiceBusMessageActions messageActions,
        FunctionContext executionContext)
    {
        var retrySender = _queueClientFactory.CreateNotifierQueueClient();
        _retry.SetContext(message, retrySender);
        await RunInternalAsync(message, messageActions, executionContext);
    }

    private async Task RunInternalAsync(ServiceBusReceivedMessage message,
                       ServiceBusMessageActions messageReceiver,
                       FunctionContext executionContext)
    {
        try
        {
            _logger.LogInformation("Messages Id : {MessageId} received on {FunctionName}", message.MessageId, executionContext.FunctionDefinition.Name);

            LogGcId(message);

            await _baseMessageProcessorService.ProcessAsync(executionContext.InvocationId,
                GcNotificationSubscriberSettings.DefaultQueueName,
                GcNotificationSubscriberSettings.PublisherId,
                message,
                messageReceiver,
                originalCrmPublisherId: GcNotificationSubscriberSettings.PublisherId,
                originalSource: GcNotificationSubscriberSettings.DefaultQueueName,
                originalRequestName: "Create");

            _logger.LogInformation("Finished processing Messages Id : {MessageId} received on {FunctionName}", message.MessageId, executionContext.FunctionDefinition.Name);
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, ex.Message);
        }
    }

    private void LogGcId(ServiceBusReceivedMessage message)
    {
        GCNotificationInbound? incoming = message.GetPayloadAsInstanceOf<GCNotificationInbound>();
        if (string.IsNullOrEmpty(incoming?.GCId))
        {
            _logger.LogWarning("The incoming message does not have a GcId");
        }
        else
        {
            _logger.LogInformation("Message received with GcId: {GcId}", incoming.GCId);
        }
    }
}
