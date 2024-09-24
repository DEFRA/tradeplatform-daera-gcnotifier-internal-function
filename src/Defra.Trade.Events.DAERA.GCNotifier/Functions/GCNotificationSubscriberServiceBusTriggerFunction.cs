// Copyright DEFRA (c). All rights reserved.
// Licensed under the Open Government License v3.0.

using System;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Defra.Trade.Common.Functions;
using Defra.Trade.Common.Functions.Extensions;
using Defra.Trade.Common.Functions.Interfaces;
using Defra.Trade.Events.DAERA.GCNotifier.Application.Dtos.Inbound;
using Defra.Trade.Events.DAERA.GCNotifier.Application.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.ServiceBus;
using Microsoft.Extensions.Logging;

namespace Defra.Trade.Events.DAERA.GCNotifier.Functions;

public sealed class GcNotificationSubscriberServiceBusTriggerFunction
{
    private readonly IBaseMessageProcessorService<GCNotificationInbound> _baseMessageProcessorService;
    private readonly IMessageRetryService _retry;

    public GcNotificationSubscriberServiceBusTriggerFunction(IBaseMessageProcessorService<GCNotificationInbound> baseMessageProcessorService, IMessageRetryService retry)
    {
        ArgumentNullException.ThrowIfNull(baseMessageProcessorService);
        ArgumentNullException.ThrowIfNull(retry);
        _baseMessageProcessorService = baseMessageProcessorService;
        _retry = retry;
    }

    [ServiceBusAccount(GcNotificationSubscriberSettings.ConnectionStringConfigurationKey)]
    [FunctionName(nameof(GcNotificationSubscriberServiceBusTriggerFunction))]
    public async Task RunAsync(
        [ServiceBusTrigger(queueName: GcNotificationSubscriberSettings.DefaultQueueName, IsSessionsEnabled = false)] ServiceBusReceivedMessage message,
        ServiceBusMessageActions messageActions,
        ExecutionContext executionContext,
        [ServiceBus(GcNotificationSubscriberSettings.TradeEventInfo)] IAsyncCollector<ServiceBusMessage> eventStoreCollector,
        [ServiceBus(GcNotificationSubscriberSettings.DefaultQueueName)] IAsyncCollector<ServiceBusMessage> retryQueue,
        ILogger logger)
    {
        _retry.SetContext(message, retryQueue);
        await RunInternalAsync(message, messageActions, eventStoreCollector, executionContext, logger);
    }

    private async Task RunInternalAsync(ServiceBusReceivedMessage message,
                       ServiceBusMessageActions messageReceiver,
                       IAsyncCollector<ServiceBusMessage> eventStoreCollector,
                       ExecutionContext executionContext,
                       ILogger logger)
    {
        try
        {
            logger.LogInformation("Messages Id : {MessageId} received on {FunctionName}", message.MessageId, executionContext.FunctionName);

            LogGcId(logger, message);

            await _baseMessageProcessorService.ProcessAsync(executionContext.InvocationId.ToString(),
                GcNotificationSubscriberSettings.DefaultQueueName,
                GcNotificationSubscriberSettings.PublisherId,
                message,
                messageReceiver,
                eventStoreCollector,
                originalCrmPublisherId: GcNotificationSubscriberSettings.PublisherId,
                originalSource: GcNotificationSubscriberSettings.DefaultQueueName,
                originalRequestName: "Create");

            logger.LogInformation("Finished processing Messages Id : {MessageId} received on {FunctionName}", message.MessageId, executionContext.FunctionName);
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, ex.Message);
        }
    }

    private static void LogGcId(ILogger logger, ServiceBusReceivedMessage message)
    {
        GCNotificationInbound? incoming = message.GetPayloadAsInstanceOf<GCNotificationInbound>();
        if (string.IsNullOrEmpty(incoming?.GCId))
        {
            logger.LogWarning("The incoming message does not have a GcId");
        }
        else
        {
            logger.LogInformation("Message received with GcId: {GcId}", incoming.GCId);
        }
    }
}
