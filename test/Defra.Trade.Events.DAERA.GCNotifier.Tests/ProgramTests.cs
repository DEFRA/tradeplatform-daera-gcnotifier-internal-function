// Copyright DEFRA (c). All rights reserved.
// Licensed under the Open Government License v3.0.

using Azure.Messaging.ServiceBus;
using Defra.Trade.Common.Function.Health.HealthChecks;
using Defra.Trade.Events.DAERA.ApiClient;
using Defra.Trade.Events.DAERA.GCNotifier.Application;
using Defra.Trade.Events.DAERA.GCNotifier.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Shouldly;

namespace Defra.Trade.Events.DAERA.GCNotifier;

public sealed class ProgramTests
{
    [Fact]
    public void AddApplication_RegistersExpectedServices()
    {
        // arrange
        var config = new ConfigurationBuilder().Build();
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<IConfiguration>(config);
        services.AddSingleton<HttpClient>();

        // act
        services.AddApplication(config);

        // assert
        var provider = services.BuildServiceProvider();
        provider.GetService<IDaeraApiClient>().ShouldNotBeNull();
        provider.GetService<IDaeraAuthenticator>().ShouldNotBeNull();
        provider.GetService<IDateTimeProvider>().ShouldNotBeNull();
    }

    [Fact]
    public void HealthChecks_RegistersExpectedChecks()
    {
        // arrange
        var config = new ConfigurationBuilder().Build();
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<IConfiguration>(config);

        var healthChecksBuilder = services.AddHealthChecks();
        healthChecksBuilder
            .AddCheck<AppSettingHealthCheck>("ServiceBus:ConnectionString")
            .AddCheck<AppSettingHealthCheck>("GCNotifier:DAERA:DaeraSubscriptionKey")
            .AddCheck<AppSettingHealthCheck>("GCNotifier:DAERA:TenantId")
            .AddCheck<AppSettingHealthCheck>("GCNotifier:DAERA:ClientId")
            .AddCheck<AppSettingHealthCheck>("GCNotifier:DAERA:PushGcEndpoint")
            .AddCheck<AppSettingHealthCheck>("GCNotifier:DAERA:Secret");

        // act
        var provider = services.BuildServiceProvider();
        var healthCheckOptions = provider.GetRequiredService<Microsoft.Extensions.Options.IOptions<HealthCheckServiceOptions>>();

        // assert
        var registrations = healthCheckOptions.Value.Registrations;
        registrations.ShouldContain(r => r.Name == "ServiceBus:ConnectionString");
        registrations.ShouldContain(r => r.Name == "GCNotifier:DAERA:DaeraSubscriptionKey");
        registrations.ShouldContain(r => r.Name == "GCNotifier:DAERA:TenantId");
        registrations.ShouldContain(r => r.Name == "GCNotifier:DAERA:ClientId");
        registrations.ShouldContain(r => r.Name == "GCNotifier:DAERA:PushGcEndpoint");
        registrations.ShouldContain(r => r.Name == "GCNotifier:DAERA:Secret");
        registrations.Count.ShouldBe(6);
    }

    [Fact]
    public void FullRegistration_ResultsInAValidConfiguration()
    {
        // arrange
        var config = new ConfigurationBuilder().Build();
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<IConfiguration>(config);
        services.AddSingleton<HttpClient>();
        services.AddSingleton(new ServiceBusClient("Endpoint=sb://test.servicebus.windows.net/;SharedAccessKeyName=test;SharedAccessKey=dGVzdA=="));
        services.ConfigureMapper();

        // act
        services.AddServiceRegistrations(config);
        services.AddApplication(config);

        var healthChecksBuilder = services.AddHealthChecks();
        healthChecksBuilder
            .AddCheck<AppSettingHealthCheck>("ServiceBus:ConnectionString")
            .AddCheck<AppSettingHealthCheck>("GCNotifier:DAERA:DaeraSubscriptionKey")
            .AddCheck<AppSettingHealthCheck>("GCNotifier:DAERA:TenantId")
            .AddCheck<AppSettingHealthCheck>("GCNotifier:DAERA:ClientId")
            .AddCheck<AppSettingHealthCheck>("GCNotifier:DAERA:PushGcEndpoint")
            .AddCheck<AppSettingHealthCheck>("GCNotifier:DAERA:Secret");

        // assert
        services.ShouldNotBeEmpty();
        services.BuildServiceProvider(new ServiceProviderOptions
        {
            ValidateOnBuild = true,
            ValidateScopes = true
        });
    }
}
