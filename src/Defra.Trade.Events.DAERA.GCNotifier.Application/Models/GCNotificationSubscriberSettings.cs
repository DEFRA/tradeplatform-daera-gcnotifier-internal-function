// Copyright DEFRA (c). All rights reserved.
// Licensed under the Open Government License v3.0.

namespace Defra.Trade.Events.DAERA.GCNotifier.Application.Models;

public sealed class GcNotificationSubscriberSettings
{
    public const string SettingsName = "GCNotifier";
#if DEBUG

    // In 'Debug' (locally) use connection string
    public const string ConnectionStringConfigurationKey = "ServiceBus:ConnectionString";

#else
    // Assumes that this is 'Release' and uses Managed Identity rather than connection string
    // ie it will actually bind to ServiceBus:FullyQualifiedNamespace !
    public const string ConnectionStringConfigurationKey = "ServiceBus";
#endif

#if DEBUG
    public const string DefaultQueueName = "defra.trade.events.remos.gcnotification.dev";
#else
    public const string DefaultQueueName = "defra.trade.events.remos.gcnotification";
#endif

    public const string PublisherId = "REMOS";
    public const string TradeEventInfo = Common.Functions.Constants.QueueName.DefaultEventsInfoQueueName;

    public const string AppConfigSentinelName = "Sentinel";
    public string RemosGCNotifierQueue { get; set; } = DefaultQueueName;

    public static class MessageRetry
    {
        public const int EnqueueTimeInSeconds = 30;
        public const int RetryWindowInSeconds = 300;
    }
}
