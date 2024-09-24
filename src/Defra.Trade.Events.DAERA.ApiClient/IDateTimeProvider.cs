// Copyright DEFRA (c). All rights reserved.
// Licensed under the Open Government License v3.0.

namespace Defra.Trade.Events.DAERA.ApiClient;

/// <summary>Retrieve DateTime information.</summary>
public interface IDateTimeProvider
{
    /// <summary>Get the DateTime as it is now.</summary>
    DateTime Now { get; }
}