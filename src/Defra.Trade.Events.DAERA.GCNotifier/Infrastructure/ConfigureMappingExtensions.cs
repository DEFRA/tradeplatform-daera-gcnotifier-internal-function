// Copyright DEFRA (c). All rights reserved.
// Licensed under the Open Government License v3.0.

using System;
using System.Linq;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace Defra.Trade.Events.DAERA.GCNotifier.Infrastructure;

public static class ConfigureMappingExtensions
{
    public static void ConfigureMapper(this IFunctionsHostBuilder hostBuilder)
    {
        var assembly = AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => a.FullName is { } fullName && fullName.Contains("Defra"))
            .OrderBy(a => a.FullName)
            .ToList();
        hostBuilder.Services.AddAutoMapper(assembly);
    }
}
