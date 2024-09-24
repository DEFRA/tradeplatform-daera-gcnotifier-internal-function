// Copyright DEFRA (c). All rights reserved.
// Licensed under the Open Government License v3.0.

using System.Collections.Generic;
using System.Text.Json;
using Defra.Trade.Events.DAERA.ApiClient.Infrastructure;
using Defra.Trade.Events.DAERA.ApiClient.Models;
using Microsoft.Extensions.Options;

namespace Defra.Trade.Events.DAERA.ApiClient;

/// <inheritdoc />
/// <summary>
/// Initializes a new instance of the <see cref="DaeraAuthenticator"/> class.
/// </summary>
/// <param name="daeraApiConfig"></param>
public class DaeraAuthenticator(IOptions<DaeraApiConfig> daeraApiConfig) : IDaeraAuthenticator
{
    private readonly DaeraApiConfig _daeraClientOptions = daeraApiConfig.Value ?? throw new ArgumentNullException(nameof(daeraApiConfig));

    /// <inheritdoc />
    public async Task<DaeraJwtTokenOptions> GenerateAuthenticationTokenAsync()
    {
        using var client = new HttpClient();

        string baseAddress = $"https://login.microsoftonline.com/{_daeraClientOptions.TenantId}/oauth2/token";
        const string grantType = "client_credentials";
        var form = new Dictionary<string, string>
                       {
                           { "grant_type", grantType },
                           { "client_id", _daeraClientOptions.ClientId },
                           { "client_secret", _daeraClientOptions.Secret }
                       };
        using var uriContext = new FormUrlEncodedContent(form);
        var tokenResponse = await client.PostAsync(baseAddress, uriContext);
        string jsonContent = await tokenResponse.Content.ReadAsStringAsync();

        tokenResponse.EnsureSuccessStatusCode();

        var jwtToken = JsonSerializer.Deserialize<DaeraJwtTokenOptions>(jsonContent);

        return jwtToken ?? throw new InvalidOperationException("Unable to get token from App reg");
    }
}
