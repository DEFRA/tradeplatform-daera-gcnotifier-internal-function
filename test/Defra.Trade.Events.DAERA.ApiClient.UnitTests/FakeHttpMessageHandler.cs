// Copyright DEFRA (c). All rights reserved.
// Licensed under the Open Government License v3.0.

namespace Defra.Trade.Events.DAERA.ApiClient.UnitTests;

public class FakeHttpMessageHandler(HttpResponseMessage response) : HttpMessageHandler
{
    private readonly Dictionary<HttpRequestMessage, DateTime> _capturedRequests = [];
    private readonly HttpResponseMessage _response = response;

    public IReadOnlyDictionary<HttpRequestMessage, DateTime> CapturedRequests => _capturedRequests;

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
                                                           CancellationToken cancellationToken)
    {
        _capturedRequests.Add(request, DateTime.UtcNow);

        return Task.FromResult(_response);
    }
}
