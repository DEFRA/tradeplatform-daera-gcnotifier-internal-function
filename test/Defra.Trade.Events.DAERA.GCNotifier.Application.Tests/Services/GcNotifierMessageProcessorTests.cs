// Copyright DEFRA (c). All rights reserved.
// Licensed under the Open Government License v3.0.

using System.Net;
using AutoFixture;
using AutoMapper;
using Azure.Messaging.ServiceBus;
using Defra.Trade.Common.Functions.Exceptions;
using Defra.Trade.Common.Functions.Interfaces;
using Defra.Trade.Common.Functions.Models;
using Defra.Trade.Events.DAERA.ApiClient;
using Defra.Trade.Events.DAERA.GCNotifier.Application.Dtos.Dynamics;
using Defra.Trade.Events.DAERA.GCNotifier.Application.Dtos.Inbound;
using Defra.Trade.Events.DAERA.GCNotifier.Tests.Common;
using FakeItEasy;
using Microsoft.Extensions.Logging;
using Moq;
using Shouldly;
using Xunit;
using APIGcNotification = Defra.Trade.Events.DAERA.ApiClient.Models.GCNotification;

namespace Defra.Trade.Events.DAERA.GCNotifier.Application.Services;

public sealed class GcNotifierMessageProcessorTests
{
    private readonly IDaeraApiClient _daeraApiClient;
    private readonly IFixture _fixture;
    private readonly Mock<ILogger<GcNotifierMessageProcessor>> _logger;
    private readonly IMapper _mapper;
    private readonly IMessageRetryContextAccessor _retry;
    private readonly GcNotifierMessageProcessor _sut;

    public GcNotifierMessageProcessorTests()
    {
        _fixture = new Fixture();
        _mapper = A.Fake<IMapper>(opt => opt.Strict());
        _logger = new Mock<ILogger<GcNotifierMessageProcessor>>();
        _logger.Setup(x => x.IsEnabled(LogLevel.Information)).Returns(true);
        _daeraApiClient = A.Fake<IDaeraApiClient>();
        _retry = A.Fake<IMessageRetryContextAccessor>(p => p.Strict());
        _sut = new GcNotifierMessageProcessor(_mapper, _daeraApiClient, _retry, _logger.Object);
    }

    [Fact]
    public async Task BuildCustomMessageHeaderAsync_ReturnsAnEmptyHeader()
    {
        // arrange

        // act
        var result = await _sut.BuildCustomMessageHeaderAsync();

        // assert
        result.ShouldBeEquivalentTo(new CustomMessageHeader());
    }

    [Fact]
    public async Task GetSchemaAsync_ReturnsAnEmptyString()
    {
        // arrange
        var header = new TradeEventMessageHeader();

        // act
        string result = await _sut.GetSchemaAsync(header);

        // assert
        result.ShouldBe("");
    }

    [Theory]
    [InlineData((HttpStatusCode)1)]
    [InlineData((HttpStatusCode)100)]
    [InlineData((HttpStatusCode)399)]
    [InlineData((HttpStatusCode)400)]
    [InlineData((HttpStatusCode)499)]
    [InlineData((HttpStatusCode)600)]
    [InlineData((HttpStatusCode)699)]
    public async Task ProcessMessage_Should_IgnoreOtherRequestExceptions(HttpStatusCode? status)
    {
        // Arrange
        var generalCertificateRequest = _fixture.Create<GcNotificationRequest>();
        var gCNotification = new GCNotification { GcId = generalCertificateRequest.GcId };
        var messageHeader = _fixture.Create<TradeEventMessageHeader>();

        var exception = new HttpRequestException("Test exception", null, status);

        A.CallTo(() => _mapper.Map<GCNotification>(generalCertificateRequest)).Returns(gCNotification);
        A.CallTo(() => _daeraApiClient.PostWithBearerTokenAsync(A<APIGcNotification>.That.Matches(x => x.GcId == generalCertificateRequest.GcId))).ThrowsAsync(exception);
        var test = () => _sut.ProcessAsync(generalCertificateRequest, messageHeader);

        // Act
        var actual = await test.ShouldThrowAsync<HttpRequestException>();

        //Assert
        A.CallTo(() => _mapper.Map<GCNotification>(generalCertificateRequest)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _daeraApiClient.PostWithBearerTokenAsync(A<APIGcNotification>.That.Matches(x => x.GcId == generalCertificateRequest.GcId))).MustHaveHappenedOnceExactly();
        _logger.VerifyLogged($"Sending GC notification with id {generalCertificateRequest.GcId} to DAERA endpoint", LogLevel.Information);
        actual.ShouldBeSameAs(exception);
    }

    [Fact]
    public async Task ProcessMessage_Should_NotRetryAConnectionError_WhenTheAttemptIsTooHigh()
    {
        // Arrange
        var generalCertificateRequest = _fixture.Create<GcNotificationRequest>();
        var gCNotification = new GCNotification { GcId = generalCertificateRequest.GcId };
        var messageHeader = _fixture.Create<TradeEventMessageHeader>();

        var messageContext = A.Fake<IMessageRetryContext>(p => p.Strict());
        var message = ServiceBusModelFactory.ServiceBusReceivedMessage(properties: new Dictionary<string, object?>
        {
            ["RetryCount"] = 10
        });
        var exception = new MessageRetryException(message.MessageId, "test exception");

        A.CallTo(() => _mapper.Map<GCNotification>(generalCertificateRequest)).Returns(gCNotification);
        A.CallTo(() => _daeraApiClient.PostWithBearerTokenAsync(A<APIGcNotification>.That.Matches(x => x.GcId == generalCertificateRequest.GcId))).ThrowsAsync(exception);
        A.CallTo(() => _retry.Context).Returns(messageContext);
        A.CallTo(() => messageContext.Message).Returns(message);
        var test = () => _sut.ProcessAsync(generalCertificateRequest, messageHeader);

        // Act
        var actual = await test.ShouldThrowAsync<MessageRetryException>();

        //Assert
        A.CallTo(() => _mapper.Map<GCNotification>(generalCertificateRequest)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _daeraApiClient.PostWithBearerTokenAsync(A<APIGcNotification>.That.Matches(x => x.GcId == generalCertificateRequest.GcId))).MustHaveHappenedOnceExactly();
        _logger.VerifyLogged($"Sending GC notification with id {generalCertificateRequest.GcId} to DAERA endpoint", LogLevel.Information);
        actual.ShouldBeSameAs(exception);
    }

    [Fact]
    public async Task ProcessMessage_Should_RetryAConnectionError_WhenTheAttemptIsNotTooHigh()
    {
        // Arrange
        var generalCertificateRequest = _fixture.Create<GcNotificationRequest>();
        var gCNotification = new GCNotification { GcId = generalCertificateRequest.GcId };
        var messageHeader = _fixture.Create<TradeEventMessageHeader>();
        var messageRetryContext = A.Fake<IMessageRetryContext>();

        A.CallTo(() => _retry.Context).Returns(messageRetryContext);
        A.CallTo(() => _mapper.Map<GCNotification>(generalCertificateRequest)).Returns(gCNotification);

        var ex = new HttpRequestException("Test API error", null, HttpStatusCode.InternalServerError);
        A.CallTo(() => _daeraApiClient.PostWithBearerTokenAsync(A<APIGcNotification>.Ignored)).ThrowsAsync(ex);

        var test = _sut.ProcessAsync(generalCertificateRequest, messageHeader);

        // Act
        var actual = await test.ShouldThrowAsync<HttpRequestException>();

        //Assert
        A.CallTo(() => _mapper.Map<GCNotification>(generalCertificateRequest)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _daeraApiClient.PostWithBearerTokenAsync(A<APIGcNotification>.Ignored)).MustHaveHappenedOnceExactly();
        actual.ShouldBeSameAs(ex);
    }

    [Fact]
    public async Task ProcessMessage_Should_SendMessage()
    {
        // Arrange
        var generalCertificateRequest = _fixture.Create<GcNotificationRequest>();
        var gCNotification = new GCNotification { GcId = generalCertificateRequest.GcId };
        var messageHeader = _fixture.Create<TradeEventMessageHeader>();

        A.CallTo(() => _mapper.Map<GCNotification>(generalCertificateRequest)).Returns(gCNotification);

        // Act
        await _sut.ProcessAsync(generalCertificateRequest, messageHeader);

        //Assert
        A.CallTo(() => _mapper.Map<GCNotification>(generalCertificateRequest)).MustHaveHappenedOnceExactly();
        _logger.VerifyLogged($"Sending GC notification with id {generalCertificateRequest.GcId} to DAERA endpoint", LogLevel.Information);
        _logger.VerifyLogged($"GC notifier with id {generalCertificateRequest.GcId} sent to DAERA endpoint", LogLevel.Information);
    }

    [Fact]
    public async Task ValidateMessageLabelAsync_ReturnsTrue()
    {
        // arrange
        var header = new TradeEventMessageHeader();

        // act
        bool result = await _sut.ValidateMessageLabelAsync(header);

        // assert
        result.ShouldBe(true);
    }
}
