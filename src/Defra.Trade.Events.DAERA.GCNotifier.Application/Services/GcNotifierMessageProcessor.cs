// Copyright DEFRA (c). All rights reserved.
// Licensed under the Open Government License v3.0.

using System.Net;
using AutoMapper;
using Defra.Trade.Common.Functions.Extensions;
using Defra.Trade.Common.Functions.Interfaces;
using Defra.Trade.Common.Functions.Models;
using Defra.Trade.Events.DAERA.ApiClient;
using Defra.Trade.Events.DAERA.GCNotifier.Application.Dtos.Dynamics;
using Defra.Trade.Events.DAERA.GCNotifier.Application.Dtos.Inbound;
using Defra.Trade.Events.DAERA.GCNotifier.Application.Extensions;
using Defra.Trade.Events.DAERA.GCNotifier.Application.Models;
using Microsoft.Extensions.Logging;

namespace Defra.Trade.Events.DAERA.GCNotifier.Application.Services;

public sealed class GcNotifierMessageProcessor : IMessageProcessor<GcNotificationRequest, TradeEventMessageHeader>
{
    private readonly IDaeraApiClient _daeraApiClient;
    private readonly ILogger<GcNotifierMessageProcessor> _logger;
    private readonly IMapper _mapper;
    private readonly TimeSpan _messageRetryEnqueueTime;
    private readonly TimeSpan _messageRetryWindow;
    private readonly IMessageRetryContextAccessor _retry;

    public GcNotifierMessageProcessor(
        IMapper mapper,
        IDaeraApiClient daeraApiClient,
        IMessageRetryContextAccessor retry,
        ILogger<GcNotifierMessageProcessor> logger)
    {
        ArgumentNullException.ThrowIfNull(mapper);
        ArgumentNullException.ThrowIfNull(daeraApiClient);
        ArgumentNullException.ThrowIfNull(retry);
        ArgumentNullException.ThrowIfNull(logger);
        _mapper = mapper;
        _logger = logger;
        _daeraApiClient = daeraApiClient;
        _retry = retry;
        _messageRetryEnqueueTime = new TimeSpan(0, 0, 0, GcNotificationSubscriberSettings.MessageRetry.EnqueueTimeInSeconds);
        _messageRetryWindow = new TimeSpan(0, 0, 0, GcNotificationSubscriberSettings.MessageRetry.RetryWindowInSeconds);
    }

    public Task<CustomMessageHeader> BuildCustomMessageHeaderAsync()
    {
        return Task.FromResult(new CustomMessageHeader());
    }

    public Task<string> GetSchemaAsync(TradeEventMessageHeader messageHeader)
    {
        return Task.FromResult(string.Empty);
    }

    public async Task<StatusResponse<GcNotificationRequest>> ProcessAsync(GcNotificationRequest messageRequest, TradeEventMessageHeader messageHeader)
    {
        _logger.StartMapping();
        var gcNotification = MapToDtoModels(messageRequest);
        _logger.CompleteMapping();

        _logger.StartSendingToDaera();
        await NotifyDaeraOfGc(gcNotification);
        _logger.CompleteSendingToDaera();

        return await Task.FromResult<StatusResponse<GcNotificationRequest>>(new()
        {
            ForwardMessage = false,
            Response = messageRequest
        });
    }

    public Task<bool> ValidateMessageLabelAsync(TradeEventMessageHeader messageHeader)
    {
        return Task.FromResult(true);
    }

    private GCNotification MapToDtoModels(GcNotificationRequest messageRequest)
    {
        return _mapper.Map<GCNotification>(messageRequest);
    }

    private async Task NotifyDaeraOfGc(GCNotification gcNotification)
    {
        var gcNotificationRequest = new ApiClient.Models.GCNotification { GcId = gcNotification.GcId };
        _logger.StartSendingNotificationToDaera(gcNotification.GcId);
        try
        {
            await _daeraApiClient.PostWithBearerTokenAsync(gcNotificationRequest);
        }
        catch (HttpRequestException ex) when (ex.StatusCode is null or 0 or (>= (HttpStatusCode)500 and <= (HttpStatusCode)599) && _retry.Context is { } context)
        {
            _logger.ProcessingFailed(ex, context.Message.MessageId, context.Message.RetryCount());

            await context.RetryMessage(_messageRetryWindow, _messageRetryEnqueueTime, ex);
        }

        _logger.CompleteSendingNotificationToDaera(gcNotification.GcId);
    }
}
