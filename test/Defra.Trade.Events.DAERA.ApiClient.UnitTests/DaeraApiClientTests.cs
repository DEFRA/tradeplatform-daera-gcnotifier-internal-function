// Copyright DEFRA (c). All rights reserved.
// Licensed under the Open Government License v3.0.

using System.Text.Json;
using Defra.Trade.Events.DAERA.ApiClient.Infrastructure;
using Defra.Trade.Events.DAERA.ApiClient.Models;
using Defra.Trade.Events.DAERA.GCNotifier.Tests.Common;
using Microsoft.Extensions.Logging;

namespace Defra.Trade.Events.DAERA.ApiClient.UnitTests;

public class DaeraApiClientTests
{
    private readonly Mock<IDaeraAuthenticator> _authenticator;
    private readonly Mock<IOptions<DaeraApiConfig>> _daeraApiConfig;
    private readonly Mock<IOptions<DaeraClientOptions>> _daeraClientOptions;
    private readonly Mock<IDateTimeProvider> _dateTimeProvider;
    private readonly Fixture _fixture;
    private readonly HttpClient _httpClient;
    private readonly Mock<ILogger<DaeraApiClient>> _loggerMock;
    private readonly DaeraApiClient _sut;
    private readonly TestHttpClientBuilder _testHttpClientBuilder;

    public DaeraApiClientTests()
    {
        _loggerMock = new Mock<ILogger<DaeraApiClient>>();
        _authenticator = new Mock<IDaeraAuthenticator>();
        _dateTimeProvider = new Mock<IDateTimeProvider>();
        _daeraClientOptions = new Mock<IOptions<DaeraClientOptions>>();
        _daeraApiConfig = new Mock<IOptions<DaeraApiConfig>>();
        _fixture = new Fixture();
        _testHttpClientBuilder = new TestHttpClientBuilder()
            .WithStatusCode(HttpStatusCode.OK)
            .WithJsonContent(_fixture.Create<DaeraApiResponse>());

        _daeraClientOptions.Setup(x => x.Value).Returns(
            new DaeraClientOptions
            {
                DaeraAuthenticationTimeoutInMinutes = 10,
                DaeraAccessToken = new DaeraJwtTokenOptions
                {
                    AccessToken = "",
                    TokenType = ""
                }
            });

        _daeraApiConfig.Setup(x => x.Value).Returns(
            new DaeraApiConfig
            {
                ClientId = "mock",
                DaeraSubscriptionKey = "mock",
                Domain = "mock",
                PushGcEndpoint = "mock",
                Secret = "mock",
                TenantId = "mock"
            });

        _httpClient = _testHttpClientBuilder.Build();

        _sut = new DaeraApiClient(
            _httpClient,
            _authenticator.Object,
            _dateTimeProvider.Object,
            _daeraClientOptions.Object,
            _daeraApiConfig.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task GetAsyncWithAuth_WhenLastAuthenticatedIsNull_ReAuthenticates()
    {
        _daeraClientOptions.Object.Value.DaeraAccessToken = null;

        var token = _fixture.Create<DaeraJwtTokenOptions>();

        _authenticator.Setup(e => e.GenerateAuthenticationTokenAsync())
            .ReturnsAsync(token);

        await _sut.PostWithBearerTokenAsync(_fixture.Create<GCNotification>());

        _authenticator.Verify(e => e.GenerateAuthenticationTokenAsync(), Times.Once);
    }

    [Theory]
    [InlineData(HttpStatusCode.BadRequest)]
    [InlineData(HttpStatusCode.InternalServerError)]
    public async Task PostAsJsonAsyncWithAuth_WhenTheResponseIsNotSuccessful_ShouldThrow(HttpStatusCode statusCode)
    {
        _daeraClientOptions.Object.Value.DaeraAccessToken = _fixture.Create<DaeraJwtTokenOptions>();
        _daeraClientOptions.Object.Value.LastAuthenticatedAt = new DateTime(2020, 05, 29, 10, 0, 0, DateTimeKind.Utc);

        var mockedDateTime = _daeraClientOptions.Object.Value.LastAuthenticatedAt.AddMinutes(5);

        _dateTimeProvider.Setup(e => e.Now)
            .Returns(mockedDateTime);

        var token = _fixture.Create<DaeraJwtTokenOptions>();

        _authenticator.Setup(e => e.GenerateAuthenticationTokenAsync())
            .ReturnsAsync(token);
        var mockGc = _fixture.Build<GCNotification>().With(x => x.Timestamp, mockedDateTime).Create();
        _testHttpClientBuilder.WithStatusCode(statusCode)
            .WithContent("Request failed", "text/plain");

        var action = () => _sut.PostWithBearerTokenAsync(mockGc);

        // Act
        var exception = await action.ShouldThrowAsync<HttpRequestException>();

        // Assert
        exception.StatusCode.ShouldBe(statusCode);
        _authenticator.Verify(e => e.GenerateAuthenticationTokenAsync(), Times.Never);

        _loggerMock.VerifyLogged("Retrieving existing token for App reg", LogLevel.Information);
        _loggerMock.VerifyLogged($"Daera endpoint {_daeraApiConfig.Object.Value.Domain}{_daeraApiConfig.Object.Value.PushGcEndpoint} with GC notification request {mockGc.GcId} returned {statusCode} with body Request failed", LogLevel.Information);
    }

    [Fact]
    public async Task PostAsJsonAsyncWithAuth_WhenTokenHasExpired_ReAuthenticates()
    {
        _daeraClientOptions.Object.Value.DaeraAccessToken = _fixture.Create<DaeraJwtTokenOptions>();
        _daeraClientOptions.Object.Value.LastAuthenticatedAt = new DateTime(2020, 05, 29, 10, 0, 0, DateTimeKind.Utc);

        var mockedDateTime = _daeraClientOptions.Object.Value.LastAuthenticatedAt.AddMinutes(
            _daeraClientOptions.Object.Value.DaeraAuthenticationTimeoutInMinutes);

        _dateTimeProvider.Setup(e => e.Now)
            .Returns(mockedDateTime);

        var token = _fixture.Create<DaeraJwtTokenOptions>();

        _authenticator.Setup(e => e.GenerateAuthenticationTokenAsync())
            .ReturnsAsync(token);
        var mockGc = _fixture.Build<GCNotification>().With(x => x.Timestamp, mockedDateTime).Create();

        // Act
        await _sut.PostWithBearerTokenAsync(mockGc);

        // Assert
        _authenticator.Verify(e => e.GenerateAuthenticationTokenAsync(), Times.Once);
        _daeraClientOptions.Object.Value.LastAuthenticatedAt.ShouldBe(_dateTimeProvider.Object.Now);
        _loggerMock.VerifyLogged($"Calling daera endpoint {_daeraApiConfig.Object.Value.Domain}{_daeraApiConfig.Object.Value.PushGcEndpoint} with GC notification request {JsonSerializer.Serialize(mockGc)}", LogLevel.Information);
        _loggerMock.VerifyLogged("Generating new token from App reg", LogLevel.Information);
    }

    [Fact]
    public async Task PostAsJsonAsyncWithAuth_WhenTokenHasNotExpired_UseTokenAndAuthenticates()
    {
        _daeraClientOptions.Object.Value.DaeraAccessToken = _fixture.Create<DaeraJwtTokenOptions>();
        _daeraClientOptions.Object.Value.LastAuthenticatedAt = new DateTime(2020, 05, 29, 10, 0, 0, DateTimeKind.Utc);

        var mockedDateTime = _daeraClientOptions.Object.Value.LastAuthenticatedAt.AddMinutes(5);

        _dateTimeProvider.Setup(e => e.Now)
            .Returns(mockedDateTime);

        var token = _fixture.Create<DaeraJwtTokenOptions>();

        _authenticator.Setup(e => e.GenerateAuthenticationTokenAsync())
            .ReturnsAsync(token);
        var mockGc = _fixture.Build<GCNotification>().With(x => x.Timestamp, mockedDateTime).Create();

        // Act
        await _sut.PostWithBearerTokenAsync(mockGc);

        // Assert
        _authenticator.Verify(e => e.GenerateAuthenticationTokenAsync(), Times.Never);

        _loggerMock.VerifyLogged("Retrieving existing token for App reg", LogLevel.Information);
    }

    [Fact]
    public void PostAsJsonAsyncWithAuth_WhenTokenIsNull_ShouldThrow()
    {
        _daeraClientOptions.Object.Value.DaeraAccessToken = _fixture.Create<DaeraJwtTokenOptions>();
        _daeraClientOptions.Object.Value.LastAuthenticatedAt = new DateTime(2020, 05, 29, 10, 0, 0, DateTimeKind.Utc);
        _dateTimeProvider.Setup(e => e.Now)
            .Returns(_daeraClientOptions.Object.Value.LastAuthenticatedAt.AddMinutes(_daeraClientOptions.Object.Value
                .DaeraAuthenticationTimeoutInMinutes));

        var token = _fixture.Build<DaeraJwtTokenOptions>().With(x => x.AccessToken, string.Empty).Create();

        _authenticator.Setup(e => e.GenerateAuthenticationTokenAsync())
            .ReturnsAsync(token);

        Func<Task> act = async () => await _sut.PostWithBearerTokenAsync(_fixture.Create<GCNotification>());

        act.ShouldThrow<InvalidOperationException>("Unable to generate token for DAERA client");
    }
}
