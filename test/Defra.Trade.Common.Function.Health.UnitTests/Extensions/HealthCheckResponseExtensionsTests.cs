// Copyright DEFRA (c). All rights reserved.
// Licensed under the Open Government License v3.0.

namespace Defra.Trade.Common.Function.Health.UnitTests.Extensions;

public class HealthCheckResponseExtensionsTests
{
    [Fact]
    public void ToResponse_NoEntries_WritesAsExpected()
    {
        // Arrange
        var entries = new Dictionary<string, HealthReportEntry>();

        var healthReport = new HealthReport(
            entries,
            HealthStatus.Healthy,
            new TimeSpan(0, 0, 1, 31));

        // Act
        var report = healthReport.ToResponse();

        // Assert
        report.Results.ShouldBeEmpty();
        report.Status.ShouldBe("Healthy");
        report.TotalDurationSeconds.ShouldBe("91.000");
    }

    [Fact]
    public void Map_WithEntries_WritesAsExpected()
    {
        // Arrange
        var exceptionData = new Dictionary<string, object> { { "key", "value" } };

        var entries = new Dictionary<string, HealthReportEntry>
        {
            {
                "healthy test", new HealthReportEntry(
                    HealthStatus.Healthy,
                    "Test description",
                    new TimeSpan(0, 0, 0, 31, 500),
                    null,
                    null)
            },
            {
                "Exception test", new HealthReportEntry(
                    HealthStatus.Unhealthy,
                    "Other",
                    new TimeSpan(0, 0, 0, 1, 500),
                    new Exception("Exception details"),
                    exceptionData)
            },
            {
                "Degraded DB", new HealthReportEntry(
                HealthStatus.Degraded,
                "Final description",
                new TimeSpan(0, 0, 1, 0, 123),
                null,
                null)
            }
        };

        var healthReport = new HealthReport(
            entries,
            HealthStatus.Degraded,
            new TimeSpan(0, 0, 0, 0, 1));

        // Act
        var report = healthReport.ToResponse();

        // Assert
        report.Results!.Count().ShouldBe(3);

        var result = report.Results!.First(r => r.Name == "healthy test");
        result.ShouldNotBeNull();
        result.Description.ShouldBe("Test description");
        result.Status.ShouldBe("Healthy");
        result.DurationSeconds.ShouldBe("31.500");
        result.Exception.ShouldBeNull();
        result.Data.ShouldBeEmpty();

        result = report.Results!.First(r => r.Name == "Exception test");
        result.ShouldNotBeNull();
        result.Description.ShouldBe("Other");
        result.Status.ShouldBe("Unhealthy");
        result.DurationSeconds.ShouldBe("1.500");
        result.Exception.ShouldBe("Exception details");
        result.Data.ShouldBe(exceptionData);

        result = report.Results!.First(r => r.Name == "Degraded DB");
        result.ShouldNotBeNull();
        result.Description.ShouldBe("Final description");
        result.Status.ShouldBe("Degraded");
        result.DurationSeconds.ShouldBe("60.123");
        result.Exception.ShouldBeNull();
        result.Data.ShouldBeEmpty();

        report.Status.ShouldBe("Degraded");
        report.TotalDurationSeconds.ShouldBe("0.001");
    }
}
