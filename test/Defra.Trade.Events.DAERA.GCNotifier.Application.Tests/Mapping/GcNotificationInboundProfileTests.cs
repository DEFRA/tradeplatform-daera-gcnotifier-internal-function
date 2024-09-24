// Copyright DEFRA (c). All rights reserved.
// Licensed under the Open Government License v3.0.

using AutoMapper;
using Defra.Trade.Events.DAERA.GCNotifier.Application.Dtos.Inbound;
using Shouldly;
using Xunit;

namespace Defra.Trade.Events.DAERA.GCNotifier.Application.Mapping;

public sealed class GcNotificationInboundProfileTests
{
    private readonly IMapper _sut;

    public GcNotificationInboundProfileTests()
    {
        _sut = new MapperConfiguration(opt =>
            {
                opt.AddProfile<GcNotificationInboundProfile>();
            }).CreateMapper();
    }

    [Fact]
    public void Map_AsExpected()
    {
        // arrange
        GCNotificationInbound inbound = new()
        {
            GCId = "mocked"
        };

        // act
        var actual = _sut.Map<GcNotificationRequest>(inbound);

        // assert
        actual.GcId.ShouldBe("mocked");
    }
}
