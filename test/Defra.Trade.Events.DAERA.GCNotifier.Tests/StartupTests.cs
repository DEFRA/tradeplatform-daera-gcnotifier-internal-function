// Copyright DEFRA (c). All rights reserved.
// Licensed under the Open Government License v3.0.

using Azure.Messaging.ServiceBus;
using Defra.Trade.Common.Functions.Isolated.Interfaces;
using Defra.Trade.Events.DAERA.ApiClient;
using Defra.Trade.Events.DAERA.GCNotifier.Application.Dtos.Inbound;
using Defra.Trade.Events.DAERA.GCNotifier.Application.Models;
using Defra.Trade.Events.DAERA.GCNotifier.Functions;
using Defra.Trade.Events.DAERA.GCNotifier.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Defra.Trade.Events.DAERA.GCNotifier;

public sealed class StartupTests
{
    [Fact]
    public void Configure_ResultsInAValidConfiguration()
    {
        // arrange
        var config = new ConfigurationBuilder()
            .Build();
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<IConfiguration>(config);
        services.AddSingleton<IDaeraApiClient, DaeraApiClient>();
        services.AddSingleton<IDaeraAuthenticator, DaeraAuthenticator>();
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
        services.AddSingleton<HttpClient>();
        services.AddSingleton(new ServiceBusClient("Endpoint=sb://test.servicebus.windows.net/;SharedAccessKeyName=test;SharedAccessKey=dGVzdA=="));
        services.ConfigureMapper();

        // act
        services.AddServiceRegistrations(config);

        // assert
        services.ShouldNotBeEmpty();
        services.BuildServiceProvider(new ServiceProviderOptions
        {
            ValidateOnBuild = true,
            ValidateScopes = true
        });
    }
}
