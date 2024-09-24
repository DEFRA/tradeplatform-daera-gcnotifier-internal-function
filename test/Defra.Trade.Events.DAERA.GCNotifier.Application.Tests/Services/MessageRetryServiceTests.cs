// Copyright DEFRA (c). All rights reserved.
// Licensed under the Open Government License v3.0.

using Azure.Messaging.ServiceBus;
using Defra.Trade.Common.Functions.Interfaces;
using Defra.Trade.Common.Functions.Services;
using FakeItEasy;
using Microsoft.Azure.WebJobs;
using Shouldly;
using Xunit;

namespace Defra.Trade.Events.DAERA.GCNotifier.Application.Services;

public class MessageRetryServiceTests
{
    private readonly MessageRetryService _sut;

    public MessageRetryServiceTests()
    {
        _sut = new MessageRetryService();
    }

    [Fact]
    public void Context_ReturnsNull_WhenNoContextHasBeenSet()
    {
        // arrange

        // act
        var actual = _sut.Context;

        // assert
        actual.ShouldBe(null);
    }

    [Fact]
    public void Context_ReturnsTheContext_WhenItIsSet()
    {
        // arrange
        var message = ServiceBusModelFactory.ServiceBusReceivedMessage();
        var queue = A.Fake<IAsyncCollector<ServiceBusMessage>>();

        // act
        _sut.SetContext(message, queue);
        var actual = _sut.Context;

        // assert
        actual.ShouldNotBe(null);
        actual!.Message.ShouldBe(message);
        actual.Queue.ShouldBe(queue);
    }

    [Fact]
    public async Task Context_ReturnsNull_WhenItIsSetOutsideOfTheCurrentAsyncScope()
    {
        // arrange
        var message = ServiceBusModelFactory.ServiceBusReceivedMessage();
        var queue = A.Fake<IAsyncCollector<ServiceBusMessage>>();
        async Task SetContext()
        {
            await Task.Delay(100);
            _sut.SetContext(message, queue);
        }

        // act
        await SetContext();
        var actual = _sut.Context;

        // assert
        actual.ShouldBe(null);
    }

    [Fact]
    public async Task Context_ReturnsTheContext_WhenItIsSetInAParentAsyncScope()
    {
        // arrange
        var message = ServiceBusModelFactory.ServiceBusReceivedMessage();
        var queue = A.Fake<IAsyncCollector<ServiceBusMessage>>();
        async Task<IMessageRetryContext?> GetContext()
        {
            await Task.Delay(100);
            return _sut.Context;
        }

        // act
        _sut.SetContext(message, queue);
        var actual = await GetContext();

        // assert
        actual.ShouldNotBe(null);
        actual!.Message.ShouldBe(message);
        actual.Queue.ShouldBe(queue);
    }
}
