// Copyright DEFRA (c). All rights reserved.
// Licensed under the Open Government License v3.0.

using AutoMapper;
using FakeItEasy;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace Defra.Trade.Events.DAERA.GCNotifier.Infrastructure;

public static class ConfigureMappingExtensionsTests
{
    [Fact]
    public static void ConfigureMapper_ShouldRegisterAValidMapper()
    {
        // arrange
        var services = new ServiceCollection();
        var builder = A.Fake<IFunctionsHostBuilder>(opt => opt.Strict());
        A.CallTo(() => builder.Services).Returns(services);

        // act
        ConfigureMappingExtensions.ConfigureMapper(builder);

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