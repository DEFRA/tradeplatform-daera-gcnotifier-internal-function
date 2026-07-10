// Copyright DEFRA (c). All rights reserved.
// Licensed under the Open Government License v3.0.

using AutoMapper;
using Microsoft.Extensions.DependencyInjection;

namespace Defra.Trade.Events.DAERA.GCNotifier.Infrastructure;

public static class ConfigureMappingExtensionsTests
{
    [Fact]
    public static void ConfigureMapper_ShouldRegisterAValidMapper()
    {
        // arrange
        var services = new ServiceCollection();

        // act
        services.ConfigureMapper();

        // assert
        var provider = services.BuildServiceProvider(new ServiceProviderOptions
        {
            ValidateOnBuild = true,
            ValidateScopes = true
        });
        var config = provider.GetRequiredService<IMapper>().ConfigurationProvider;
        config.AssertConfigurationIsValid();
    }
}
