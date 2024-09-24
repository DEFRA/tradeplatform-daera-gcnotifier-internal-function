// Copyright DEFRA (c). All rights reserved.
// Licensed under the Open Government License v3.0.

using Shouldly;
using Xunit;

namespace Defra.Trade.Events.DAERA.GCNotifier.Application.Models;

public class GcNotificationSubscriberSettingsTests
{
    [Fact]
    public void Options_ShouldBe_AsExpected()
    {
        // Act
        var sut = new GcNotificationSubscriberSettings
        {
            RemosGCNotifierQueue = "defra.trade.events.remos.gcnotification"
        };

        // Assert
        sut.RemosGCNotifierQueue.ShouldBe("defra.trade.events.remos.gcnotification");
    }
}