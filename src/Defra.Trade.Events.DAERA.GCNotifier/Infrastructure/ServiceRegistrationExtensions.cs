// Copyright DEFRA (c). All rights reserved.
// Licensed under the Open Government License v3.0.

using Defra.Trade.Common.Config;
using Defra.Trade.Common.Functions;
using Defra.Trade.Common.Functions.EventStore;
using Defra.Trade.Common.Functions.Interfaces;
using Defra.Trade.Common.Functions.Models;
using Defra.Trade.Common.Functions.Services;
using Defra.Trade.Common.Functions.Validation;
using Defra.Trade.Events.DAERA.GCNotifier.Application.Dtos.Inbound;
using Defra.Trade.Events.DAERA.GCNotifier.Application.Models;
using Defra.Trade.Events.DAERA.GCNotifier.Application.Services;
using Defra.Trade.Events.DAERA.GCNotifier.Application.Validators;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Defra.Trade.Events.DAERA.GCNotifier.Infrastructure;
public static class ServiceRegistrationExtensions
{
    public static IServiceCollection AddServiceRegistrations(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<ICustomValidatorFactory, CustomValidatorFactory>();
        services.AddSingleton<AbstractValidator<TradeEventMessageHeader>, GcNotifierEventMessageHeaderValidator>();
        services.AddSingleton<AbstractValidator<GCNotificationInbound>, GcNotificationInboundValidator>();
        services.AddEventStoreConfiguration();

        services.AddTransient<IMessageProcessor<GcNotificationRequest, TradeEventMessageHeader>, GcNotifierMessageProcessor>();
        services.AddTransient<IInboundMessageValidator<GCNotificationInbound, TradeEventMessageHeader>,
            InboundMessageValidator<GCNotificationInbound, GcNotificationRequest, TradeEventMessageHeader>>();
        services.AddTransient<ISchemaValidator, SchemaValidator>();
        services.AddTransient<IBaseMessageProcessorService<GCNotificationInbound>,
            BaseMessageProcessorService<GCNotificationInbound, GcNotificationRequest, GcNotificationRequest, TradeEventMessageHeader>>();

        services.AddSingleton<IMessageCollector, EventStoreCollector>();

        services.AddMessageRetryService();

        var gcConfig = configuration.GetSection(GcNotificationSubscriberSettings.SettingsName);
        services.AddOptions<GcNotificationSubscriberSettings>().Bind(gcConfig);

        services.Configure<ServiceBusSettings>(configuration.GetSection(ServiceBusSettings.OptionsName));
        return services;
    }

    private static IServiceCollection AddMessageRetryService(this IServiceCollection services)
    {
        return services
            .AddSingleton<MessageRetryService>()
            .AddSingleton<IMessageRetryService>(p => p.GetRequiredService<MessageRetryService>())
            .AddSingleton<IMessageRetryContextAccessor>(p => p.GetRequiredService<MessageRetryService>());
    }
}
