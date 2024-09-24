// Copyright DEFRA (c). All rights reserved.
// Licensed under the Open Government License v3.0.

using Defra.Trade.Events.DAERA.ApiClient.Models;

namespace Defra.Trade.Events.DAERA.ApiClient;

/// <summary>
/// Http client authenticator.
/// </summary>
public interface IDaeraAuthenticator
{
    /// <summary>
    /// Get auth token.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<DaeraJwtTokenOptions> GenerateAuthenticationTokenAsync();
}