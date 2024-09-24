// Copyright DEFRA (c). All rights reserved.
// Licensed under the Open Government License v3.0.

using Defra.Trade.Common.Functions.Interfaces;
using Defra.Trade.Common.Functions.Services;
using Defra.Trade.Events.DAERA.ApiClient;
using Defra.Trade.Events.DAERA.GCNotifier.Functions;
using FakeItEasy;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Defra.Trade.Events.DAERA.GCNotifier;

public sealed class StartupTests
{
    private readonly Startup _sut;

    public StartupTests()
    {
        _sut = new Startup();
    }

    [Fact]
    public void Configure_ResultsInAValidConfiguration()
    {
        // arrange
        var context = new WebJobsBuilderContext();
        var webJobs = A.Fake<IWebJobsBuilder>(opt => opt.Strict());
        var config = new ConfigurationBuilder()
            .Build();
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<IDaeraApiClient, DaeraApiClient>();
        services.AddSingleton<IDaeraAuthenticator, DaeraAuthenticator>();
        services.AddSingleton<IDaeraAuthenticator, DaeraAuthenticator>();
        services.AddSingleton<HttpClient>();
        services.AddScoped<GcNotificationSubscriberServiceBusTriggerFunction>();
        services.AddSingleton<IMessageRetryService, MessageRetryService>();

        context.Configuration = config;
        services.AddSingleton(context.Configuration);
        A.CallTo(() => webJobs.Services).Returns(services);

        var builder = CreateHostBuilder(context, webJobs);

        // act
        _sut.Configure(builder);

        // assert
        services.ShouldNotBeEmpty();
        services.BuildServiceProvider(new ServiceProviderOptions
        {
            ValidateOnBuild = true,
            ValidateScopes = true
        });
    }

    private static IFunctionsHostBuilder CreateHostBuilder(WebJobsBuilderContext context, IWebJobsBuilder webJobs)
    {
        var startup = A.Fake<FunctionsStartup>(opt => opt.Strict());
        IFunctionsHostBuilder? functionsBuilder = null;
        var captureFunctionsHostBuilder = (IFunctionsHostBuilder b) =>
        {
            functionsBuilder = b;
            return true;
        };
        A.CallTo(() => startup.Configure(A<IFunctionsHostBuilder>.That.Matches(captureFunctionsHostBuilder, "NO-OP"))).DoesNothing();
        startup.Configure(context, webJobs);
        return functionsBuilder!;
    }
}
