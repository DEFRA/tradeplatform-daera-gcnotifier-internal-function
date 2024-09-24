// Copyright DEFRA (c). All rights reserved.
// Licensed under the Open Government License v3.0.

using System.Text;
using Defra.Trade.Events.DAERA.ApiClient.Infrastructure;
using Defra.Trade.Events.DAERA.ApiClient.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Defra.Trade.Events.DAERA.ApiClient;

/// <summary>
/// Initializes a new instance of the <see cref="DaeraApiClient"/> class.
/// </summary>
/// <param name="apiClient"></param>
/// <param name="daeraAuthenticator"></param>
/// <param name="dateTimeProvider"></param>
/// <param name="DaeraClientOptions"></param>
/// <param name="DaeraApiConfig"></param>
public class DaeraApiClient(
    HttpClient apiClient,
    IDaeraAuthenticator daeraAuthenticator,
    IDateTimeProvider dateTimeProvider,
    IOptions<DaeraClientOptions> DaeraClientOptions,
    IOptions<DaeraApiConfig> DaeraApiConfig,
    ILogger<DaeraApiClient> logger) : IDaeraApiClient
{
    private readonly DaeraClientOptions _daeraClientOptions = DaeraClientOptions.Value;
    private readonly DaeraApiConfig _daeraApiConfig = DaeraApiConfig.Value;

    /// <inheritdoc />
    public async Task<string> PostWithBearerTokenAsync(GCNotification gcNotification)
    {
        string daeraPushGcEndpoint = $"{_daeraApiConfig.Domain}{_daeraApiConfig.PushGcEndpoint}";
        string daeraAuthToken = await SetAuthenticationAsync();

        if (string.IsNullOrWhiteSpace(daeraAuthToken))
        {
            throw new InvalidOperationException("Unable to generate token for DAERA client");
        }

        gcNotification.Timestamp = dateTimeProvider.Now;

        string gcNotificationJson = JsonConvert.SerializeObject(gcNotification);
        var gcNotificationJsonStringContent = new StringContent(gcNotificationJson, Encoding.UTF8, "application/json");
        apiClient.DefaultRequestHeaders.Clear();
        apiClient.DefaultRequestHeaders.Add("Authorization", $"bearer {daeraAuthToken}");
        apiClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", _daeraApiConfig.DaeraSubscriptionKey);

        logger.LogInformation("Calling daera endpoint {DaeraPushGcEndpoint} with GC notification request {GcNotificationJson}",
            daeraPushGcEndpoint,
            gcNotificationJson);

        var daeraApiResponse = await apiClient.PostAsync(daeraPushGcEndpoint, gcNotificationJsonStringContent);
        if (!daeraApiResponse.IsSuccessStatusCode)
        {
            string responseString = await daeraApiResponse.Content.ReadAsStringAsync();
            logger.LogInformation(
                "Daera endpoint {DaeraPushGcEndpoint} with GC notification request {GcNotificationJson} returned {StatusCode} with body {ResponseBody}",
                daeraPushGcEndpoint,
                gcNotification.GcId,
                daeraApiResponse.StatusCode,
                responseString);
        }
        daeraApiResponse.EnsureSuccessStatusCode();
        return daeraApiResponse.ReasonPhrase;
    }

    /// <summary>
    /// Method to get bearer token from App reg.
    /// </summary>
    /// <returns>Bearer token.</returns>
    /// <exception cref="InvalidOperationException"></exception>
    private async Task<string> SetAuthenticationAsync()
    {
        var tokenExpiryTime = _daeraClientOptions.LastAuthenticatedAt.AddMinutes(_daeraClientOptions.DaeraAuthenticationTimeoutInMinutes);
        if (!string.IsNullOrEmpty(_daeraClientOptions.DaeraAccessToken?.AccessToken)
            && tokenExpiryTime > dateTimeProvider.Now)
        {
            logger.LogInformation("Retrieving existing token for App reg");
            return _daeraClientOptions.DaeraAccessToken?.AccessToken;
        }

        logger.LogInformation("Generating new token from App reg");
        var token = await daeraAuthenticator.GenerateAuthenticationTokenAsync()
                    ?? throw new InvalidOperationException("Unable to get token from Azure App reg.");

        _daeraClientOptions.DaeraAccessToken = token;
        _daeraClientOptions.LastAuthenticatedAt = dateTimeProvider.Now;

        return token.AccessToken;
    }
}
