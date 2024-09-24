// Copyright DEFRA (c). All rights reserved.
// Licensed under the Open Government License v3.0.

using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;

namespace Defra.Trade.Events.DAERA.GCNotifier.Helpers;

public class FakeHttpRequestData(
#pragma warning disable CS9113 // Used in unit tests.
    FunctionContext functionContext,
    Uri url,
#pragma warning restore CS9113 // Used in unit tests.
    Stream? body = null) : HttpRequest
{
    public override string? ContentType { get; set; } = string.Empty;

    public override Stream Body { get; set; } = body ?? new MemoryStream();

    public override bool HasFormContentType { get; } = false;

    public override IFormCollection Form { get; set; } = new FormCollection(null, null);

    public override string Protocol { get; set; } = string.Empty;

    public override IHeaderDictionary Headers { get; } = new HeaderDictionary();

    public override IRequestCookieCollection Cookies { get; set; } = null!;

    public override long? ContentLength { get; set; } = new long();

    public override Task<IFormCollection> ReadFormAsync(CancellationToken cancellationToken = new CancellationToken()) => throw new NotImplementedException();

    public override HttpContext HttpContext { get; } = new DefaultHttpContext();

    public override string Method { get; set; } = string.Empty;

    public override string Scheme { get; set; } = string.Empty;

    public override bool IsHttps { get; set; }

    public override HostString Host { get; set; }

    public override PathString PathBase { get; set; }

    public override PathString Path { get; set; }

    public override QueryString QueryString { get; set; }

    public override IQueryCollection Query { get; set; } = new QueryCollection();
}
