// Copyright DEFRA (c). All rights reserved.
// Licensed under the Open Government License v3.0.

using System.Text.Json.Serialization;

namespace Defra.Trade.Events.DAERA.ApiClient.Models;

public class DaeraJwtTokenOptions
{
    /// <summary>
    /// Bearer token.
    /// </summary>
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; } = string.Empty;

    /// <summary>
    /// Token type.
    /// </summary>
    [JsonPropertyName("token_type")]
    public string TokenType { get; set; } = string.Empty;
}