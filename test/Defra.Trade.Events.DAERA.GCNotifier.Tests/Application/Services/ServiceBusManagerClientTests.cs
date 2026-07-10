// Copyright DEFRA (c). All rights reserved.
// Licensed under the Open Government License v3.0.

using Azure.Messaging.ServiceBus;
using Defra.Trade.Events.DAERA.GCNotifier.Application.Services;
using Defra.Trade.Events.DAERA.GCNotifier.Application.Services.Contracts;
using FakeItEasy;
using Shouldly;

namespace Defra.Trade.Events.DAERA.GCNotifier.Application.Services;

public sealed class ServiceBusManagerClientTests
{
    private readonly IQueueClientFactory _queueClientFactory;
    private readonly ServiceBusManagerClient _sut;

    public ServiceBusManagerClientTests()
    {
        _queueClientFactory = A.Fake<IQueueClientFactory>(opt => opt.Strict());
        _sut = new ServiceBusManagerClient(_queueClientFactory);
    }

    [Fact]
    public void Ctor_NullQueueClientFactory_ThrowsArgumentNullException()
    {
        var act = () => new ServiceBusManagerClient(null!);

        act.ShouldThrow<ArgumentNullException>().ParamName.ShouldBe("queueClientFactory");
    }

    [Fact]
    public async Task SendMessageAsync_CreatesClientAndSendsMessage()
    {
        // Arrange
        var message = new ServiceBusMessage("test-body");
        var sender = A.Fake<ServiceBusSender>();
        A.CallTo(() => _queueClientFactory.CreateNotifierQueueClient()).Returns(sender);
        A.CallTo(() => sender.SendMessageAsync(message, A<CancellationToken>._)).Returns(Task.CompletedTask);

        // Act
        await _sut.SendMessageAsync(message);

        // Assert
        A.CallTo(() => _queueClientFactory.CreateNotifierQueueClient()).MustHaveHappenedOnceExactly();
        A.CallTo(() => sender.SendMessageAsync(message, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task SendMessageAsync_UsesCorrectSenderFromFactory()
    {
        // Arrange
        var message = new ServiceBusMessage("payload");
        var sender = A.Fake<ServiceBusSender>();
        A.CallTo(() => _queueClientFactory.CreateNotifierQueueClient()).Returns(sender);
        A.CallTo(() => sender.SendMessageAsync(message, A<CancellationToken>._)).Returns(Task.CompletedTask);

        // Act
        await _sut.SendMessageAsync(message);

        // Assert
        A.CallTo(() => sender.SendMessageAsync(message, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
    }
}
