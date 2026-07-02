// Copyright DEFRA (c). All rights reserved.
// Licensed under the Open Government License v3.0.

using Azure.Messaging.ServiceBus;
using Defra.Trade.Events.DAERA.GCNotifier.Application.Models;
using Defra.Trade.Events.DAERA.GCNotifier.Application.Services;
using FakeItEasy;
using Microsoft.Extensions.Options;
using Shouldly;

namespace Defra.Trade.Events.DAERA.GCNotifier.Application.Services;

public sealed class QueueClientFactoryTests
{
    private readonly IOptions<GcNotificationSubscriberSettings> _settings;
    private readonly ServiceBusClient _client;
    private readonly QueueClientFactory _sut;

    public QueueClientFactoryTests()
    {
        _settings = A.Fake<IOptions<GcNotificationSubscriberSettings>>();
        _client = A.Fake<ServiceBusClient>();

        A.CallTo(() => _settings.Value).Returns(new GcNotificationSubscriberSettings
        {
            RemosGCNotifierQueue = "test-queue"
        });

        _sut = new QueueClientFactory(_settings, _client);
    }

    [Fact]
    public void Ctor_NullSettings_ThrowsArgumentNullException()
    {
        var act = () => new QueueClientFactory(null!, _client);

        act.ShouldThrow<ArgumentNullException>().ParamName.ShouldBe("settings");
    }

    [Fact]
    public void Ctor_NullClient_ThrowsArgumentNullException()
    {
        var act = () => new QueueClientFactory(_settings, null!);

        act.ShouldThrow<ArgumentNullException>().ParamName.ShouldBe("client");
    }

    [Fact]
    public void CreateNotifierQueueClient_ReturnsSenderForConfiguredQueue()
    {
        // Arrange
        var expectedSender = A.Fake<ServiceBusSender>();
        A.CallTo(() => _client.CreateSender("test-queue")).Returns(expectedSender);

        // Act
        var result = _sut.CreateNotifierQueueClient();

        // Assert
        result.ShouldBe(expectedSender);
        A.CallTo(() => _client.CreateSender("test-queue")).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void CreateNotifierQueueClient_CalledMultipleTimes_ReturnsCachedSender()
    {
        // Arrange
        var expectedSender = A.Fake<ServiceBusSender>();
        A.CallTo(() => _client.CreateSender("test-queue")).Returns(expectedSender);

        // Act
        var first = _sut.CreateNotifierQueueClient();
        var second = _sut.CreateNotifierQueueClient();

        // Assert
        first.ShouldBe(second);
        A.CallTo(() => _client.CreateSender("test-queue")).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task DisposeAsync_DisposesAllSenders()
    {
        // Arrange
        var sender = A.Fake<ServiceBusSender>();
        A.CallTo(() => _client.CreateSender("test-queue")).Returns(sender);
        _sut.CreateNotifierQueueClient();

        // Act
        await _sut.DisposeAsync();

        // Assert
        A.CallTo(() => sender.DisposeAsync()).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task DisposeAsync_NoSendersCreated_CompletesWithoutError()
    {
        // Act & Assert
        await _sut.DisposeAsync();
    }
}
