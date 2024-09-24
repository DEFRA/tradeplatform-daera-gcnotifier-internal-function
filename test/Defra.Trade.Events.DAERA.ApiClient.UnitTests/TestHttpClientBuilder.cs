// Copyright DEFRA (c). All rights reserved.
// Licensed under the Open Government License v3.0.

using System.Text;
using Newtonsoft.Json;

namespace Defra.Trade.Events.DAERA.ApiClient.UnitTests;

public class TestHttpClientBuilder
{
    private readonly HttpResponseMessage _stubHttpResponseMessage = new(HttpStatusCode.OK);

    public TestHttpClient Build()
    {
        return new TestHttpClient(
            new FakeHttpMessageHandler(_stubHttpResponseMessage));
    }

    public TestHttpClientBuilder WithContent(string content, string contentType)
    {
        _stubHttpResponseMessage.Content = new StringContent(content, Encoding.UTF8, contentType);
        return this;
    }

    public TestHttpClientBuilder WithJsonContent<T>(T expectedResponseObject)
    {
        _stubHttpResponseMessage.Content = new StringContent(JsonConvert.SerializeObject(expectedResponseObject),
            Encoding.UTF8, "application/json");
        return this;
    }

    public TestHttpClientBuilder WithStatusCode(HttpStatusCode statusCode)
    {
        _stubHttpResponseMessage.StatusCode = statusCode;
        return this;
    }

    public class TestHttpClient : HttpClient
    {
        private readonly FakeHttpMessageHandler _httpMessageHandler;

        public IReadOnlyDictionary<HttpRequestMessage, DateTime> CapturedRequests =>
            _httpMessageHandler.CapturedRequests;

        internal TestHttpClient(FakeHttpMessageHandler httpMessageHandler) : base(httpMessageHandler)
        {
            _httpMessageHandler = httpMessageHandler;
            BaseAddress = new Uri("http://localhost.com");
        }
    }
}
