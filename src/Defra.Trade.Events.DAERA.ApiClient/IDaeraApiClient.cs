// Copyright DEFRA (c). All rights reserved.
// Licensed under the Open Government License v3.0.


using Defra.Trade.Events.DAERA.ApiClient.Models;

namespace Defra.Trade.Events.DAERA.ApiClient;
/// <summary>
/// Daera Api client
/// </summary>
public interface IDaeraApiClient
{
    /// <summary>
    /// Post to Daera Api.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<string> PostWithBearerTokenAsync(GCNotification gcNotification);
}
