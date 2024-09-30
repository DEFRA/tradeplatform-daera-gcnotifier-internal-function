// Copyright DEFRA (c). All rights reserved.
// Licensed under the Open Government License v3.0.

using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Text;
using Defra.Trade.Events.DAERA.ApiClient;
using Defra.Trade.Events.DAERA.ApiClient.Infrastructure;
using Defra.Trade.Events.DAERA.ApiClient.Models;

namespace Defra.Trade.Common.Function.Health.HealthChecks.ApiCheck;

/// <summary>
/// Health check for Trade Api,
/// </summary>
[ExcludeFromCodeCoverage(Justification = "Health check extensions cant be unit tested")]
public class DaeraApiHealthCheck(ServiceProvider serviceProvider) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        await Task.Delay(5, cancellationToken).ConfigureAwait(false);
        var result = await ExecuteCheckAsync(context, cancellationToken);
        return result;
    }

    private async Task<HealthCheckResult> ExecuteCheckAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        HealthCheckResult result;
        try
        {
            result = await CheckHealthInternalAsync(context, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            result = HealthCheckResult.Unhealthy("The health check operation timed out");
        }
        catch (Exception ex)
        {
            result = HealthCheckResult.Unhealthy($"Exception during check: {ex.GetType().FullName}", ex);
        }

        return result;
    }

    protected async Task<HealthCheckResult> CheckHealthInternalAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            string name = context.Registration.Name;
            var daeraApiConfig = serviceProvider.GetRequiredService<IOptions<DaeraApiConfig>>();
            string authToken = await SetAuthenticationAsync();

            var apiClient = new HttpClient();
            string daeraPushGcEndpoint = $"{daeraApiConfig.Value.Domain}{daeraApiConfig.Value.PushGcEndpoint}";

            var gcNotificationJsonStringContent = new StringContent("{}", Encoding.UTF8, "application/json");
            apiClient.DefaultRequestHeaders.Clear();
            apiClient.DefaultRequestHeaders.Add("Authorization", $"bearer {authToken}");
            apiClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", daeraApiConfig.Value.DaeraSubscriptionKey);

            var daeraApiResponse = await apiClient.PostAsync(daeraPushGcEndpoint, gcNotificationJsonStringContent, cancellationToken);
            bool isCreated = daeraApiResponse.StatusCode == HttpStatusCode.Created;


            var responseData = new Dictionary<string, object>
                               {
                                   {"url", daeraPushGcEndpoint},
                                   {"name", name},
                                   {"StatusCode", daeraApiResponse.StatusCode}
                               };
            return isCreated
                       ? HealthCheckResult.Healthy("Healthy", responseData)
                       : HealthCheckResult.Unhealthy($"Exception during check Daera Api check with status code: {daeraApiResponse.StatusCode}", null, responseData);
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy($"Exception during check: {ex.GetType().FullName}", ex, new Dictionary<string, object> { { "url", "DaeraApi" } });
        }
    }

    private async Task<string> SetAuthenticationAsync()
    {
        var daeraAuthenticator = serviceProvider.GetRequiredService<IDaeraAuthenticator>();
        var daeraClientOptions = serviceProvider.GetRequiredService<IOptions<DaeraClientOptions>>();

        var tokenExprityTime = daeraClientOptions.Value.LastAuthenticatedAt.AddMinutes(daeraClientOptions.Value.DaeraAuthenticationTimeoutInMinutes);
        if (!string.IsNullOrEmpty(daeraClientOptions.Value.DaeraAccessToken?.AccessToken)
            && tokenExprityTime > DateTime.Now)
        {

            return daeraClientOptions.Value.DaeraAccessToken?.AccessToken;
        }

        var token = await daeraAuthenticator.GenerateAuthenticationTokenAsync()
                    ?? throw new InvalidOperationException("Unable to get token from Azure App reg.");

        daeraClientOptions.Value.DaeraAccessToken = token;
        daeraClientOptions.Value.LastAuthenticatedAt = DateTime.Now;

        return token.AccessToken;
    }
}
