// Copyright DEFRA (c). All rights reserved.
// Licensed under the Open Government License v3.0.

using System.ComponentModel.DataAnnotations;

namespace Defra.Trade.Events.DAERA.ApiClient.Infrastructure;

public sealed class DaeraApiConfig
{
    /// <summary>
    /// The unique identifier of the Azure AD instance.
    /// </summary>
    [Required]
    public string TenantId { get; set; } = string.Empty;

    /// <summary>
    /// The unique identifier of the registered application we are connecting to in the Azure AD instance.
    /// </summary>
    [Required]
    public string ClientId { get; set; } = string.Empty;

    /// <summary>
    /// The unique secret of the registered application we are connecting to in the Azure AD instance.
    /// </summary>
    [Required]
    public string Secret { get; set; } = string.Empty;

    /// <summary>
    /// The issuer/domain of the registered application we are connecting to in the Azure AD instance.
    /// </summary>
    [Required]
    public string Domain { get; set; } = string.Empty;

    /// <summary>
    /// Get transactional data endpoint.
    /// </summary>
    public string PushGcEndpoint { get; set; } = string.Empty;

    /// <summary>
    /// Daera subscription key.
    /// </summary>
    public string DaeraSubscriptionKey { get; set; } = string.Empty;

    /// <summary>
    /// Daera App config.
    /// </summary>
    public const string SectionName = "GCNotifier:DAERA";
}