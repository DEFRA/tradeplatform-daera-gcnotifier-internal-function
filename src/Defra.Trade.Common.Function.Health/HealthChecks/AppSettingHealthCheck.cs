// Copyright DEFRA (c). All rights reserved.
// Licensed under the Open Government License v3.0.

namespace Defra.Trade.Common.Function.Health.HealthChecks;

/// <summary>
/// A health check that checks if an app setting is present.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="AppSettingHealthCheck"/> class.
/// </remarks>
/// <param name="configuration">IConfiguration instance to load app setting.</param>
public class AppSettingHealthCheck(IConfiguration configuration) : IHealthCheck
{

    /// <inheritdoc/>
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        string name = context.Registration.Name;
        var result = HealthCheckResult.Unhealthy($"Error occurred validating app setting {name} as it is null or empty");

        string value = configuration[name];
        if (!string.IsNullOrWhiteSpace(value))
        {
            result = HealthCheckResult.Healthy($"Successfully validated app setting {name}");
        }

        return Task.FromResult(result);
    }
}
