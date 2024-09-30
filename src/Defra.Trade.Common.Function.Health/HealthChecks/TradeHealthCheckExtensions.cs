// Copyright DEFRA (c). All rights reserved.
// Licensed under the Open Government License v3.0.

using System.Diagnostics.CodeAnalysis;
using Defra.Trade.Common.Function.Health.HealthChecks.ApiCheck;

namespace Defra.Trade.Common.Function.Health.HealthChecks;

[ExcludeFromCodeCoverage(Justification = "Unable to mock service provider. This can be looked at later stage")]
public static class TradeHealthCheckExtensions
{
    /// <summary>
    /// Health check for Daera API.
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="serviceProvider"></param>
    /// <returns></returns>
    public static IHealthChecksBuilder AddDaeraApiCheck(
        this IHealthChecksBuilder builder,
        ServiceProvider serviceProvider)
    {
        builder.Add(new HealthCheckRegistration(
            "DaeraApi",
            sp => new DaeraApiHealthCheck(serviceProvider),
            failureStatus: default,
            tags: default,
            timeout: default));

        return builder;
    }

    public static IHealthChecksBuilder AddAzureServiceBusCheck(
        this IHealthChecksBuilder builder,
        IConfiguration configuration,
        string serviceBusConnectionConfigPath,
        string queueName)
    {
        string servicesBusConnectionString = configuration.GetValue<string>(serviceBusConnectionConfigPath);
        string servicesBusQueueName = queueName;

        builder.Add(new HealthCheckRegistration(
           $"ServiceBus:{queueName}",
            sp => new ServiceBusQueueHealthCheck(servicesBusConnectionString, servicesBusQueueName),
            failureStatus: default,
            tags: default,
            timeout: default));
        return builder;
    }
}
