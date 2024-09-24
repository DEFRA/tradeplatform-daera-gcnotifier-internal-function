// Copyright DEFRA (c). All rights reserved.
// Licensed under the Open Government License v3.0.

using AutoMapper;
using Defra.Trade.Events.DAERA.GCNotifier.Application.Dtos.Dynamics;
using Defra.Trade.Events.DAERA.GCNotifier.Application.Dtos.Inbound;
using Shouldly;
using Xunit;

namespace Defra.Trade.Events.DAERA.GCNotifier.Application.Mapping;

public sealed class GcNotificationRequestProfileTests
{
    private readonly IMapper _sut;

    public GcNotificationRequestProfileTests()
    {
        _sut = new MapperConfiguration(opt =>
        {
            opt.AddProfile<GcNotificationRequestProfile>();
        }).CreateMapper();
    }

    [Fact]
    public void Map_AsExpected()
    {
        // arrange
        GcNotificationRequest inbound = new()
        {
            GcId = "mocked"
        };

        // act
        var actual = _sut.Map<GCNotification>(inbound);

        // assert
        actual.GcId.ShouldBe("mocked");
    }
}
