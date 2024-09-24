// Copyright DEFRA (c). All rights reserved.
// Licensed under the Open Government License v3.0.

namespace Defra.Trade.Events.DAERA.ApiClient.UnitTests;

public class DateTimeProviderTests
{
    [Fact]
    public void Now_WhenCalled_ReturnsUtcNow()
    {
        // Arrange
        var sut = new DateTimeProvider();

        // Act
        var utcNow = DateTime.UtcNow;
        var result = sut.Now;

        // Assert
        (utcNow.AddSeconds(-2) < result && result < utcNow.AddSeconds(2)).ShouldBeTrue();
    }
}