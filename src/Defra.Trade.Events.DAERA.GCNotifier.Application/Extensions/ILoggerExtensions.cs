// Copyright DEFRA (c). All rights reserved.
// Licensed under the Open Government License v3.0.

using Microsoft.Extensions.Logging;

namespace Defra.Trade.Events.DAERA.GCNotifier.Application.Extensions;

public static partial class ILoggerExtensions
{
    [LoggerMessage(EventId = 0, Level = LogLevel.Information, Message = "Mapping inbound messages")]
    public static partial void StartMapping(this ILogger logger);

    [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "Mapping inbound messages succeeded")]
    public static partial void CompleteMapping(this ILogger logger);

    [LoggerMessage(EventId = 2, Level = LogLevel.Information, Message = "Sending GC notification to DAERA")]
    public static partial void StartSendingToDaera(this ILogger logger);

    [LoggerMessage(EventId = 3, Level = LogLevel.Information, Message = "Sending GC notification to DAERA is completed")]
    public static partial void CompleteSendingToDaera(this ILogger logger);

    [LoggerMessage(EventId = 4, Level = LogLevel.Information, Message = "Sending GC notification with id {GcId} to DAERA endpoint")]
    public static partial void StartSendingNotificationToDaera(this ILogger logger, string? gcId);

    [LoggerMessage(EventId = 5, Level = LogLevel.Error, Message = "Processing failed for messageId: {MessageId}. Retry count: {RetryCount}")]
    public static partial void ProcessingFailed(this ILogger logger, Exception ex, string messageId, int retryCount);

    [LoggerMessage(EventId = 6, Level = LogLevel.Information, Message = "GC notifier with id {GcId} sent to DAERA endpoint")]
    public static partial void CompleteSendingNotificationToDaera(this ILogger logger, string? gcId);
}
