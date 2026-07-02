// Copyright DEFRA (c). All rights reserved.
// Licensed under the Open Government License v3.0.

using Azure.Messaging.ServiceBus;
using Defra.Trade.Common.Functions.Isolated;
using Defra.Trade.Common.Functions.Isolated.Interfaces;
using Defra.Trade.Events.DAERA.GCNotifier.Application.Dtos.Inbound;
using Defra.Trade.Events.DAERA.GCNotifier.Application.Models;
using Defra.Trade.Events.DAERA.GCNotifier.Application.Services.Contracts;
using FakeItEasy;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Defra.Trade.Events.DAERA.GCNotifier.Functions;

public sealed class GcNotificationSubscriberServiceBusTriggerFunctionTests
{
    private readonly IBaseMessageProcessorService<GCNotificationInbound> _processor;
    private readonly IMessageRetryService _retry;
    private readonly IQueueClientFactory _queueClientFactory;
    private readonly ILogger<GcNotificationSubscriberServiceBusTriggerFunction> _logger;
    private readonly GcNotificationSubscriberServiceBusTriggerFunction _sut;

    public GcNotificationSubscriberServiceBusTriggerFunctionTests()
    {
        _processor = A.Fake<IBaseMessageProcessorService<GCNotificationInbound>>(opt => opt.Strict());
        _retry = A.Fake<IMessageRetryService>(opt => opt.Strict());
        _queueClientFactory = A.Fake<IQueueClientFactory>();
        _logger = A.Fake<ILogger<GcNotificationSubscriberServiceBusTriggerFunction>>();

        _sut = new GcNotificationSubscriberServiceBusTriggerFunction(_processor, _retry, _queueClientFactory, _logger);
    }

    [Fact]
    public async Task RunAsync_CallsTheProcessorWithTheCorrectArguments()
    {
        // arrange
        string messageId = Guid.NewGuid().ToString();
        string invocationId = Guid.NewGuid().ToString();
        string functionName = Guid.NewGuid().ToString();
        var message = ServiceBusModelFactory.ServiceBusReceivedMessage(messageId: messageId, body: BinaryData.FromString("{\"GCId\": \"123\"}"));

        var actions = A.Fake<ServiceBusMessageActions>(opt => opt.Strict());
        var context = A.Fake<FunctionContext>();
        var functionDefinition = A.Fake<FunctionDefinition>();
        A.CallTo(() => context.InvocationId).Returns(invocationId);
        A.CallTo(() => context.FunctionDefinition).Returns(functionDefinition);
        A.CallTo(() => functionDefinition.Name).Returns(functionName);

        var retrySender = A.Fake<ServiceBusSender>();
        A.CallTo(() => _queueClientFactory.CreateNotifierQueueClient()).Returns(retrySender);

        var setRetryContext = A.CallTo(() => _retry.SetContext(message, retrySender));
        var processAsyncCall = A.CallTo(() => _processor.ProcessAsync(
            invocationId,
            GcNotificationSubscriberSettings.DefaultQueueName,
            GcNotificationSubscriberSettings.PublisherId,
            message,
            actions,
            null,
            null,
            GcNotificationSubscriberSettings.PublisherId,
            GcNotificationSubscriberSettings.DefaultQueueName,
            "Create"
        ));

        processAsyncCall.Returns(true);
        setRetryContext.DoesNothing();

        // act
        await _sut.RunAsync(message, actions, context);

        // assert
        setRetryContext.MustHaveHappenedOnceExactly();
        processAsyncCall.MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task RunAsync_LogsWhenTheProcessorThrows()
    {
        // arrange
        string messageId = Guid.NewGuid().ToString();
        string invocationId = Guid.NewGuid().ToString();
        string functionName = Guid.NewGuid().ToString();
        var message = ServiceBusModelFactory.ServiceBusReceivedMessage(messageId: messageId);
        var actions = A.Fake<ServiceBusMessageActions>(opt => opt.Strict());
        var context = A.Fake<FunctionContext>();
        var functionDefinition = A.Fake<FunctionDefinition>();
        A.CallTo(() => context.InvocationId).Returns(invocationId);
        A.CallTo(() => context.FunctionDefinition).Returns(functionDefinition);
        A.CallTo(() => functionDefinition.Name).Returns(functionName);

        var retrySender = A.Fake<ServiceBusSender>();
        A.CallTo(() => _queueClientFactory.CreateNotifierQueueClient()).Returns(retrySender);

        var exception = new Exception("abc");

        var setRetryContext = A.CallTo(() => _retry.SetContext(message, retrySender));
        var processAsyncCall = A.CallTo(() => _processor.ProcessAsync(
            invocationId,
            GcNotificationSubscriberSettings.DefaultQueueName,
            GcNotificationSubscriberSettings.PublisherId,
            message,
            actions,
            null,
            null,
            GcNotificationSubscriberSettings.PublisherId,
            GcNotificationSubscriberSettings.DefaultQueueName,
            "Create"
        ));

        processAsyncCall.Throws(exception);
        setRetryContext.DoesNothing();

        // act
        await _sut.RunAsync(message, actions, context);

        // assert
        setRetryContext.MustHaveHappenedOnceExactly();
        processAsyncCall.MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task RunAsync_CallsTheProcessorWithTheCorrectArgumentsButNoGcId()
    {
        // arrange
        string messageId = Guid.NewGuid().ToString();
        string invocationId = Guid.NewGuid().ToString();
        string functionName = Guid.NewGuid().ToString();
        var message = ServiceBusModelFactory.ServiceBusReceivedMessage(messageId: messageId, body: BinaryData.FromString("{\"AProperty\": \"123\"}"));

        var actions = A.Fake<ServiceBusMessageActions>(opt => opt.Strict());
        var context = A.Fake<FunctionContext>();
        var functionDefinition = A.Fake<FunctionDefinition>();
        A.CallTo(() => context.InvocationId).Returns(invocationId);
        A.CallTo(() => context.FunctionDefinition).Returns(functionDefinition);
        A.CallTo(() => functionDefinition.Name).Returns(functionName);

        var retrySender = A.Fake<ServiceBusSender>();
        A.CallTo(() => _queueClientFactory.CreateNotifierQueueClient()).Returns(retrySender);

        var setRetryContext = A.CallTo(() => _retry.SetContext(message, retrySender));
        var processAsyncCall = A.CallTo(() => _processor.ProcessAsync(
            invocationId,
            GcNotificationSubscriberSettings.DefaultQueueName,
            GcNotificationSubscriberSettings.PublisherId,
            message,
            actions,
            null,
            null,
            GcNotificationSubscriberSettings.PublisherId,
            GcNotificationSubscriberSettings.DefaultQueueName,
            "Create"
        ));

        processAsyncCall.Returns(true);
        setRetryContext.DoesNothing();

        // act
        await _sut.RunAsync(message, actions, context);

        // assert
        setRetryContext.MustHaveHappenedOnceExactly();
        processAsyncCall.MustHaveHappenedOnceExactly();
    }
}
