// Copyright DEFRA (c). All rights reserved.
// Licensed under the Open Government License v3.0.

using Defra.Trade.Events.DAERA.GCNotifier.Application.Dtos.Inbound;
using Shouldly;
using Xunit;

namespace Defra.Trade.Events.DAERA.GCNotifier.Application.Validators;

public class GcNotificationInboundValidatorTests
{
    private readonly GcNotificationInboundValidator _validator = new();

    [Fact]
    public void Should_Validate_GCId()
    {
        // Arrange
        string propertyNameToValidate = "GCId";

        // Act
        var result = _validator.Validate(new GCNotificationInbound
        {
            GCId = null
        });

        // Assert
        result.Errors.Count(x => x.PropertyName == propertyNameToValidate).ShouldBe(1);
        result.Errors.Exists(x => x.PropertyName == propertyNameToValidate).ShouldBeTrue();
    }

    [Fact]
    public void Should_Validate_GCId_WithNoErrors()
    {
        // Arrange
        string propertyNameToValidate = "GCId";

        // Act
        var result = _validator.Validate(new GCNotificationInbound
        {
            GCId = "mocked"
        });

        // Assert
        result.Errors.Count(x => x.PropertyName == propertyNameToValidate).ShouldBe(0);
        result.Errors.Exists(x => x.PropertyName == propertyNameToValidate).ShouldBeFalse();
    }
}
