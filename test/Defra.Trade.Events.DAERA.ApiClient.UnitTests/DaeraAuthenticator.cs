// Copyright DEFRA (c). All rights reserved.
// Licensed under the Open Government License v3.0.

using Defra.Trade.Events.DAERA.ApiClient.Infrastructure;

namespace Defra.Trade.Events.DAERA.ApiClient.UnitTests;

public class DaeraAuthenticatorTests
{
    private readonly Mock<IOptions<DaeraApiConfig>> _daeraClientOptions;

    public DaeraAuthenticatorTests()
    {
        _daeraClientOptions = new Mock<IOptions<DaeraApiConfig>>();
    }

    [Fact]
    public void PostWithBearerTokenAsync_WhenCalled_ShouldReturnToken()
    {
        // Arrange
        _daeraClientOptions.Setup(x => x.Value).Returns(
            new DaeraApiConfig
            {
                ClientId = "mock",
                DaeraSubscriptionKey = "mock",
                Domain = "mock",
                PushGcEndpoint = "mock",
                Secret = "mock",
                TenantId = "mock"
            });
        var sut = new DaeraAuthenticator(_daeraClientOptions.Object);

        // Act
        var getToken = sut.GenerateAuthenticationTokenAsync();

        // Assert
        getToken.ShouldNotBeNull();
    }

    [Fact]
    public Task PostWithBearerTokenAsync_WhenInvalidConfig_ShouldNotReturnToken()
    {
        // Arrange
        _daeraClientOptions.Setup(x => x.Value).Returns(
            new DaeraApiConfig
            {
                ClientId = null,
                DaeraSubscriptionKey = "mock",
                Domain = "mock",
                PushGcEndpoint = "mock",
                Secret = "mock",
                TenantId = "mock"
            });
        var sut = new DaeraAuthenticator(_daeraClientOptions.Object);

        // Act
        Func<Task> act = async () => await sut.GenerateAuthenticationTokenAsync();

        // Assert
        act.ShouldThrow<HttpRequestException>();
        return Task.CompletedTask;
    }
}
