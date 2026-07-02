// Copyright DEFRA (c). All rights reserved.
// Licensed under the Open Government License v3.0.

using System.Diagnostics.CodeAnalysis;

using Defra.Trade.Common.AppConfig;
using Defra.Trade.Common.Config;
using Defra.Trade.Common.Function.Health.HealthChecks;
using Defra.Trade.Events.DAERA.ApiClient.Infrastructure;
using Defra.Trade.Events.DAERA.GCNotifier.Application;
using Defra.Trade.Events.DAERA.GCNotifier.Application.Models;
using Defra.Trade.Events.DAERA.GCNotifier.Infrastructure;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

[assembly: ExcludeFromCodeCoverage]

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

builder.Configuration.ConfigureTradeAppConfiguration(opt =>
{
    opt.UseKeyVaultSecrets = true;
    opt.RefreshKeys.Add($"{GcNotificationSubscriberSettings.SettingsName}:{GcNotificationSubscriberSettings.AppConfigSentinelName}");
    opt.Select<ConfigurationServerSettings>(ConfigurationServerSettings.OptionsName);
    opt.Select<ServiceBusSettings>(ServiceBusSettings.OptionsName);
    opt.Select<GcNotificationSubscriberSettings>(GcNotificationSubscriberSettings.SettingsName);
    opt.Select<DaeraApiConfig>(DaeraApiConfig.SectionName);
});

var configuration = builder.Configuration;

builder.Services.AddApplicationInsightsTelemetryWorkerService();
builder.Services.ConfigureFunctionsApplicationInsights();

builder.Services.AddTradeAppConfiguration(configuration);
builder.Services.AddServiceRegistrations(configuration);
builder.Services.AddApplication(configuration);
builder.Services.ConfigureMapper();

var healthChecksBuilder = builder.Services.AddHealthChecks();
healthChecksBuilder
    .AddCheck<AppSettingHealthCheck>("ServiceBus:ConnectionString")
    .AddCheck<AppSettingHealthCheck>("GCNotifier:DAERA:DaeraSubscriptionKey")
    .AddCheck<AppSettingHealthCheck>("GCNotifier:DAERA:TenantId")
    .AddCheck<AppSettingHealthCheck>("GCNotifier:DAERA:ClientId")
    .AddCheck<AppSettingHealthCheck>("GCNotifier:DAERA:PushGcEndpoint")
    .AddCheck<AppSettingHealthCheck>("GCNotifier:DAERA:Secret");

await builder.Build().RunAsync();
