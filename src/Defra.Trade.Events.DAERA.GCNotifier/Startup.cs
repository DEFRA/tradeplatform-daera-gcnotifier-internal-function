// Copyright DEFRA (c). All rights reserved.
// Licensed under the Open Government License v3.0.

using Defra.Trade.Common.AppConfig;
using Defra.Trade.Common.Config;
using Defra.Trade.Common.Function.Health.HealthChecks;
using Defra.Trade.Common.Logging.Extensions;
using Defra.Trade.Events.DAERA.ApiClient.Infrastructure;
using Defra.Trade.Events.DAERA.GCNotifier;
using Defra.Trade.Events.DAERA.GCNotifier.Application;
using Defra.Trade.Events.DAERA.GCNotifier.Application.Models;
using Defra.Trade.Events.DAERA.GCNotifier.Infrastructure;
using FunctionHealthCheck;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(Startup))]

namespace Defra.Trade.Events.DAERA.GCNotifier;

public sealed class Startup : FunctionsStartup
{
    public override void Configure(IFunctionsHostBuilder builder)
    {
        var configuration = builder.GetContext().Configuration;

        builder.Services.AddTradeAppConfiguration(configuration);
        builder.Services.AddServiceRegistrations(configuration);
        builder.Services.AddApplication(configuration);
        builder.Services.AddFunctionLogging("RemosGCNotifier");
        var healthChecksBuilder = builder.Services.AddFunctionHealthChecks();
        RegisterHealthChecks(healthChecksBuilder, configuration);
        builder.ConfigureMapper();
    }

    public override void ConfigureAppConfiguration(IFunctionsConfigurationBuilder builder)
    {
        builder.ConfigurationBuilder
           .ConfigureTradeAppConfiguration(opt =>
           {
               opt.UseKeyVaultSecrets = true;
               opt.RefreshKeys.Add($"{GcNotificationSubscriberSettings.SettingsName}:{GcNotificationSubscriberSettings.AppConfigSentinelName}");
               opt.Select<ConfigurationServerSettings>(ConfigurationServerSettings.OptionsName);
               opt.Select<ServiceBusSettings>(ServiceBusSettings.OptionsName);
               opt.Select<GcNotificationSubscriberSettings>(GcNotificationSubscriberSettings.SettingsName);
               opt.Select<DaeraApiConfig>(DaeraApiConfig.SectionName);
           });
    }

    private static void RegisterHealthChecks(
       IHealthChecksBuilder builder,
       IConfiguration configuration)
    {
        builder.AddCheck<AppSettingHealthCheck>("ServiceBus:ConnectionString")
            .AddCheck<AppSettingHealthCheck>("GCNotifier:DAERA:DaeraSubscriptionKey")
            .AddCheck<AppSettingHealthCheck>("GCNotifier:DAERA:TenantId")
            .AddCheck<AppSettingHealthCheck>("GCNotifier:DAERA:ClientId")
            .AddCheck<AppSettingHealthCheck>("GCNotifier:DAERA:PushGcEndpoint")
            .AddCheck<AppSettingHealthCheck>("GCNotifier:DAERA:Secret");

        builder.AddAzureServiceBusCheck(configuration, "ServiceBus:ConnectionString", GcNotificationSubscriberSettings.DefaultQueueName);
    }
}
