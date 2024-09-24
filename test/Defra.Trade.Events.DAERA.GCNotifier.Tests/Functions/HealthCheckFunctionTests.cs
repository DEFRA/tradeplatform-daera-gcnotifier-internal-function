// Copyright DEFRA (c). All rights reserved.
// Licensed under the Open Government License v3.0.

using Defra.Trade.Common.Function.Health;
using Defra.Trade.Events.DAERA.GCNotifier.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Defra.Trade.Events.DAERA.GCNotifier.Functions;

public class HealthCheckFunctionTests
{
    private readonly Mock<HealthCheckService> _healthCheckService;
    private readonly HealthCheckFunction _sut;

    public HealthCheckFunctionTests()
    {
        _healthCheckService = new Mock<HealthCheckService>();
        _sut = new HealthCheckFunction(_healthCheckService.Object);
    }

    [Fact]
    public void RunAsync_HasFunctionAttribute()
    {
        // Arrange & Act
        var attribute = FunctionTestHelpers.MethodHasSingleAttribute<HealthCheckFunction, FunctionNameAttribute>(
            nameof(HealthCheckFunction.RunAsync));

        // Assert
        attribute.Name.ShouldBe("HealthCheckFunction");
    }

    [Fact]
    public void RunAsync_HasHttpTriggerAttributeWithCorrectValues()
    {
        // Arrange & Act & Assert
        FunctionTestHelpers.Function_HasHttpTriggerAttributeWithCorrectValues<HealthCheckFunction>(
            nameof(HealthCheckFunction.RunAsync),
            "health",
            ["GET"],
            AuthorizationLevel.Anonymous);
    }

    [Fact]
    public async Task RunAsync_ValidHealthCheck_ReturnsOkResponse()
    {
        // Arrange
        var body = new MemoryStream(Encoding.UTF8.GetBytes(string.Empty));
        var req = new FakeHttpRequestData(new Mock<FunctionContext>().Object, new Uri("https://test/api/message"), body);
        var healthReport = new HealthReport(new Dictionary<string, HealthReportEntry>(), HealthStatus.Healthy, TimeSpan.FromSeconds(1));
        _healthCheckService.Setup(s => s.CheckHealthAsync(null, CancellationToken.None)).ReturnsAsync(healthReport);

        // Act
        var result = await _sut.RunAsync(req);

        // Assert
        result.ShouldNotBeNull();
        var bodyText = result as JsonResult;
        bodyText.ShouldNotBeNull();
        bodyText.Value.ShouldBe("Healthy");
    }

    [Fact]
    public async Task RunAsync_InvalidHealthCheck_ReturnsInternalServerErrorResponse()
    {
        // Arrange
        var body = new MemoryStream(Encoding.UTF8.GetBytes(string.Empty));
        var req = new FakeHttpRequestData(new Mock<FunctionContext>().Object, new Uri("https://test/api/message"), body);
        var healthReport = new HealthReport(new Dictionary<string, HealthReportEntry>(), HealthStatus.Unhealthy, TimeSpan.FromSeconds(1));
        _healthCheckService.Setup(s => s.CheckHealthAsync(null, CancellationToken.None)).ReturnsAsync(healthReport);

        // Act
        var result = await _sut.RunAsync(req);

        // Assert
        result.ShouldNotBeNull();
        var bodyText = result as JsonResult;
        bodyText.ShouldNotBeNull();
        bodyText.Value.ShouldNotBeNull();
        var errors = bodyText.Value as HealthCheckResponse;
        errors.ShouldNotBeNull();
        errors.Status.ShouldBe("Unhealthy");
    }
}
