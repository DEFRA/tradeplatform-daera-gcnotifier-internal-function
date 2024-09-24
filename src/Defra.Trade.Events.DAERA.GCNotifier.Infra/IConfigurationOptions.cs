// Copyright DEFRA (c). All rights reserved.
// Licensed under the Open Government License v3.0.

using System.Diagnostics.CodeAnalysis;

namespace Defra.Trade.Events.DAERA.GCNotifier.Infra;

/// <summary>
/// Provides abstraction for configuration options.
/// </summary>
public interface IConfigurationOptions
{
    /// <summary>
    /// Name of a configuration section.
    /// </summary>
    [ExcludeFromCodeCoverage(Justification = "Unit tests not required at this stage.")]
    public static string SectionName { get; }
}