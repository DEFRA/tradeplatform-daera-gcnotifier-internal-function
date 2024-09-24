// Copyright DEFRA (c). All rights reserved.
// Licensed under the Open Government License v3.0.

using System.Text.Json.Serialization;

namespace Defra.Trade.Common.Function.Health;

/// <summary>
/// Information about the health of a WebAPI.
/// </summary>
public class HealthCheckResponse
{
    /// <summary>
    /// The overall status of the health check.
    /// </summary>
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Amount of time that the response took to execute.
    /// </summary>
    public string TotalDurationSeconds { get; set; } = string.Empty;

    /// <summary>
    /// Health check details.
    /// </summary>
    public IEnumerable<HealthCheckResponseEntry> Results { get; set; }
}