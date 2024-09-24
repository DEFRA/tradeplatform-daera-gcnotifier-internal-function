// Copyright DEFRA (c). All rights reserved.
// Licensed under the Open Government License v3.0.

using Defra.Trade.Events.DAERA.ApiClient.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Defra.Trade.Events.DAERA.ApiClient;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDaeraApiClient(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddTransient<IDaeraAuthenticator, DaeraAuthenticator>();
        services.AddTransient<IDaeraApiClient, DaeraApiClient>();
        services.AddTransient<IDateTimeProvider, DateTimeProvider>();

        var apiConfig = configuration.GetSection("GCNotifier:DAERA");
        services.AddOptions<DaeraApiConfig>().Bind(apiConfig);
        return services;
    }
}