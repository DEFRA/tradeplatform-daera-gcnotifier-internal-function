// Copyright DEFRA (c). All rights reserved.
// Licensed under the Open Government License v3.0.

using System.Diagnostics.CodeAnalysis;
using Microsoft.Azure.ServiceBus;

namespace Defra.Trade.Common.Function.Health.HealthChecks;

/// <summary>
/// Health check for Trade Api,
/// </summary>
[ExcludeFromCodeCoverage]
public class ServiceBusQueueHealthCheck(string serviceBusConConfig, string queueName) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        await Task.Delay(5, cancellationToken).ConfigureAwait(false);
        var result = await ExecuteCheckAsync(context, cancellationToken);
        return result;
    }

    private async Task<HealthCheckResult> ExecuteCheckAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        HealthCheckResult result;
        try
        {
            result = await CheckHealthInternalAsync(context, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            result = HealthCheckResult.Unhealthy("The health check operation timed out");
        }
        catch (Exception ex)
        {
            result = HealthCheckResult.Unhealthy($"Exception during check: {ex.GetType().FullName}", ex);
        }

        return result;
    }

#pragma warning disable IDE0060 // Remove unused parameter
    protected Task<HealthCheckResult> CheckHealthInternalAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
#pragma warning restore IDE0060 // Remove unused parameter
    {
        try
        {
            string name = context.Registration.Name;
            var client = CreateQueueClient(serviceBusConConfig);
            _ = client.ServiceBusConnection.Endpoint;
            return Task.FromResult(HealthCheckResult.Healthy($"{name} Service bus connection successful."));
        }
        catch (Exception ex)
        {
            var data = new Dictionary<string, object> { { "url", queueName + "/health" } };
            return Task.FromResult(HealthCheckResult.Unhealthy($"Exception during check: {ex.GetType().FullName}", ex, data));
        }
    }

    public IQueueClient CreateQueueClient(string connectionString)
    {
        return new QueueClient(connectionString, queueName, ReceiveMode.PeekLock, RetryExponential.Default);
    }

}
