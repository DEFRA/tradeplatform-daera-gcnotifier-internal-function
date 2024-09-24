// Copyright DEFRA (c). All rights reserved.
// Licensed under the Open Government License v3.0.

#nullable enable

namespace Defra.Trade.Events.DAERA.ApiClient.Models;

public class DaeraClientOptions
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DaeraClientOptions"/> class.
    /// </summary>
    public DaeraClientOptions()
    {
        DaeraAuthenticationTimeoutInMinutes = 10;
    }

    /// <summary>
    /// Token expiry in minutes.
    /// </summary>
    public int DaeraAuthenticationTimeoutInMinutes { get; set; }

    /// <summary>
    /// Last token generated.
    /// </summary>
    public DateTime LastAuthenticatedAt { get; set; }

    /// <summary>
    /// Access token info.
    /// </summary>
    public DaeraJwtTokenOptions? DaeraAccessToken { get; set; }

    /// <summary>
    /// Config section name.
    /// </summary>
    public string SectionName => nameof(SectionName);
}