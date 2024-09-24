// Copyright DEFRA (c). All rights reserved.
// Licensed under the Open Government License v3.0.

using Defra.Trade.Common.Functions.Models;
using Defra.Trade.Common.Functions.Models.Enum;
using Shouldly;
using Xunit;

namespace Defra.Trade.Events.DAERA.GCNotifier.Application.Validators;

public class GcNotifierEventMessageHeaderValidatorTests
{
    private readonly GcNotifierEventMessageHeaderValidator _validator;
    private readonly string _validGuid;
    private readonly string _validContentType;
    private readonly string _invalidContentType;
    private readonly string _validLabel;
    private readonly string _invalidLabel;

    public GcNotifierEventMessageHeaderValidatorTests()
    {
        _validator = new GcNotifierEventMessageHeaderValidator();

        _validGuid = Guid.NewGuid().ToString();
        _validContentType = "application/json";
        _invalidContentType = "invalid/type";
        _validLabel = "trade.remos.notification";
        _invalidLabel = "trade.invalid.label";
    }

    [Fact]
    public void Should_Validate_MessageId_Is_Null_Returns_ErrorMessage()
    {
        // Arrange
        string propertyNameToValidate = "MessageId";

        // Act
        var result = _validator.Validate(new TradeEventMessageHeader
        {
            MessageId = null
        });

        // Assert
        result.Errors.Count(x => x.PropertyName == propertyNameToValidate).ShouldBe(1);
        result.Errors.Exists(x => x.PropertyName == propertyNameToValidate).ShouldBeTrue();
    }

    [Fact]
    public void Should_Validate_MessageId_Is_Empty_Returns_ErrorMessage()
    {
        // Arrange
        string propertyNameToValidate = "MessageId";

        // Act
        var result = _validator.Validate(new TradeEventMessageHeader
        {
            MessageId = string.Empty
        });

        // Assert
        result.Errors.Count(x => x.PropertyName == propertyNameToValidate).ShouldBe(1);
        result.Errors.Exists(x => x.PropertyName == propertyNameToValidate).ShouldBeTrue();
    }

    [Fact]
    public void Should_Validate_MessageId_Is_A_Guid_Returns_No_ErrorMessage()
    {
        // Arrange
        string propertyNameToValidate = "MessageId";

        // Act
        var result = _validator.Validate(new TradeEventMessageHeader
        {
            MessageId = _validGuid
        });

        // Assert
        result.Errors.Count(x => x.PropertyName == propertyNameToValidate).ShouldBe(0);
        result.Errors.Exists(x => x.PropertyName == propertyNameToValidate).ShouldBeFalse();
    }

    [Fact]
    public void Should_Validate_CausationId_Is_Null_Returns_ErrorMessage()
    {
        // Arrange
        string propertyNameToValidate = "CausationId";

        // Act
        var result = _validator.Validate(new TradeEventMessageHeader
        {
            CausationId = null
        });

        // Assert
        result.Errors.Count(x => x.PropertyName == propertyNameToValidate).ShouldBe(1);
        result.Errors.Exists(x => x.PropertyName == propertyNameToValidate).ShouldBeTrue();
    }

    [Fact]
    public void Should_Validate_CausationId_Is_Empty_Returns_ErrorMessage()
    {
        // Arrange
        string propertyNameToValidate = "CausationId";

        // Act
        var result = _validator.Validate(new TradeEventMessageHeader
        {
            CausationId = string.Empty
        });

        // Assert
        result.Errors.Count(x => x.PropertyName == propertyNameToValidate).ShouldBe(1);
        result.Errors.Exists(x => x.PropertyName == propertyNameToValidate).ShouldBeTrue();
    }

    [Fact]
    public void Should_Validate_CausationId_Is_A_Guid_Returns_No_ErrorMessage()
    {
        // Arrange
        string propertyNameToValidate = "CausationId";

        // Act
        var result = _validator.Validate(new TradeEventMessageHeader
        {
            CausationId = _validGuid
        });

        // Assert
        result.Errors.Count(x => x.PropertyName == propertyNameToValidate).ShouldBe(0);
        result.Errors.Exists(x => x.PropertyName == propertyNameToValidate).ShouldBeFalse();
    }

    [Fact]
    public void Should_Validate_CorrelationId_Is_Null_Returns_ErrorMessage()
    {
        // Arrange
        string propertyNameToValidate = "CorrelationId";

        // Act
        var result = _validator.Validate(new TradeEventMessageHeader
        {
            CorrelationId = null
        });

        // Assert
        result.Errors.Count(x => x.PropertyName == propertyNameToValidate).ShouldBe(1);
        result.Errors.Exists(x => x.PropertyName == propertyNameToValidate).ShouldBeTrue();
    }

    [Fact]
    public void Should_Validate_CorrelationId_Is_Empty_Returns_ErrorMessage()
    {
        // Arrange
        string propertyNameToValidate = "CorrelationId";

        // Act
        var result = _validator.Validate(new TradeEventMessageHeader
        {
            CorrelationId = string.Empty
        });

        // Assert
        result.Errors.Count(x => x.PropertyName == propertyNameToValidate).ShouldBe(1);
        result.Errors.Exists(x => x.PropertyName == propertyNameToValidate).ShouldBeTrue();
    }

    [Fact]
    public void Should_Validate_CorrelationId_Is_A_Guid_Returns_No_ErrorMessage()
    {
        // Arrange
        string propertyNameToValidate = "CorrelationId";

        // Act
        var result = _validator.Validate(new TradeEventMessageHeader
        {
            CorrelationId = _validGuid
        });

        // Assert
        result.Errors.Count(x => x.PropertyName == propertyNameToValidate).ShouldBe(0);
        result.Errors.Exists(x => x.PropertyName == propertyNameToValidate).ShouldBeFalse();
    }

    [Fact]
    public void Should_Validate_UserId_Is_A_Guid_Returns_No_ErrorMessage()
    {
        // Arrange
        string propertyNameToValidate = "UserId";

        // Act
        var result = _validator.Validate(new TradeEventMessageHeader
        {
            UserId = _validGuid
        });

        // Assert
        result.Errors.Count(x => x.PropertyName == propertyNameToValidate).ShouldBe(0);
        result.Errors.Exists(x => x.PropertyName == propertyNameToValidate).ShouldBeFalse();
    }

    [Fact]
    public void Should_Validate_ContentType_Is_Invalid_Type_Returns_ErrorMessage()
    {
        // Arrange
        string propertyNameToValidate = "ContentType";

        // Act
        var result = _validator.Validate(new TradeEventMessageHeader
        {
            ContentType = _invalidContentType
        });

        // Assert
        result.Errors.Count(x => x.PropertyName == propertyNameToValidate).ShouldBe(1);
        result.Errors.Exists(x => x.PropertyName == propertyNameToValidate).ShouldBeTrue();
    }

    [Fact]
    public void Should_Validate_ContentType_Is_Valid_Type_Returns_No_ErrorMessage()
    {
        // Arrange
        string propertyNameToValidate = "ContentType";

        // Act
        var result = _validator.Validate(new TradeEventMessageHeader
        {
            ContentType = _validContentType
        });

        // Assert
        result.Errors.Count(x => x.PropertyName == propertyNameToValidate).ShouldBe(0);
        result.Errors.Exists(x => x.PropertyName == propertyNameToValidate).ShouldBeFalse();
    }

    [Fact]
    public void Should_Validate_Label_Is_Null_Returns_ErrorMessage()
    {
        // Arrange
        string propertyNameToValidate = "Label";

        // Act
        var result = _validator.Validate(new TradeEventMessageHeader
        {
            Label = null
        });

        // Assert
        result.Errors.Count(x => x.PropertyName == propertyNameToValidate).ShouldBe(1);
        result.Errors.Exists(x => x.PropertyName == propertyNameToValidate).ShouldBeTrue();
    }

    [Fact]
    public void Should_Validate_Label_Is_Empty_Returns_ErrorMessage()
    {
        // Arrange
        string propertyNameToValidate = "Label";

        // Act
        var result = _validator.Validate(new TradeEventMessageHeader
        {
            Label = string.Empty
        });

        // Assert
        result.Errors.Count(x => x.PropertyName == propertyNameToValidate).ShouldBe(1);
        result.Errors.Exists(x => x.PropertyName == propertyNameToValidate).ShouldBeTrue();
    }

    [Fact]
    public void Should_Validate_Label_Is_Invalid_Returns_ErrorMessage()
    {
        // Arrange
        string propertyNameToValidate = "Label";

        // Act
        var result = _validator.Validate(new TradeEventMessageHeader
        {
            Label = _invalidLabel
        });

        // Assert
        result.Errors.Count(x => x.PropertyName == propertyNameToValidate).ShouldBe(1);
        result.Errors.Exists(x => x.PropertyName == propertyNameToValidate).ShouldBeTrue();
    }

    [Fact]
    public void Should_Validate_Label_Is_Valid_Returns_No_ErrorMessage()
    {
        // Arrange
        string propertyNameToValidate = "Label";

        // Act
        var result = _validator.Validate(new TradeEventMessageHeader
        {
            Label = _validLabel
        });

        // Assert
        result.Errors.Count(x => x.PropertyName == propertyNameToValidate).ShouldBe(0);
        result.Errors.Exists(x => x.PropertyName == propertyNameToValidate).ShouldBeFalse();
    }

    [Fact]
    public void ValidHeader_ShouldNotHaveAnyErrors()
    {
        // Arrange
        string contentType = "application/json";
        string messageId = Guid.NewGuid().ToString();
        string correlationId = Guid.NewGuid().ToString();
        string causationId = Guid.NewGuid().ToString();
        string entityKey = "123456";
        string publisherId = "gcenricher";
        var type = EventType.Internal;
        int timestamp = 20231212;
        string label = "trade.remos.notification";
        string schemaVersion = "1";
        string status = "Created";

        // Act
        var result = _validator.Validate(new TradeEventMessageHeader
        {
            MessageId = messageId,
            ContentType = contentType,
            TimestampUtc = timestamp,
            SchemaVersion = schemaVersion,
            CorrelationId = correlationId,
            CausationId = causationId,
            EntityKey = entityKey,
            Label = label,
            PublisherId = publisherId,
            Type = type,
            Status = status
        });

        // Assert
        result.Errors.Count.ShouldBe(0);
    }

    [Fact]
    public void ValidHeader_ShouldHaveAnyErrors()
    {
        // Act
        var result = _validator.Validate(new TradeEventMessageHeader());

        // Assert
        result.Errors.Count.ShouldBe(10);
        result.Errors.Exists(x => x.PropertyName == "MessageId").ShouldBeTrue();
        result.Errors.Exists(x => x.PropertyName == "ContentType").ShouldBeTrue();
        result.Errors.Exists(x => x.PropertyName == "TimestampUtc").ShouldBeTrue();
        result.Errors.Exists(x => x.PropertyName == "SchemaVersion").ShouldBeTrue();
        result.Errors.Exists(x => x.PropertyName == "CorrelationId").ShouldBeTrue();
        result.Errors.Exists(x => x.PropertyName == "CausationId").ShouldBeTrue();
        result.Errors.Exists(x => x.PropertyName == "EntityKey").ShouldBeTrue();
        result.Errors.Exists(x => x.PropertyName == "Label").ShouldBeTrue();
        result.Errors.Exists(x => x.PropertyName == "PublisherId").ShouldBeTrue();
        result.Errors.Exists(x => x.PropertyName == "Type").ShouldBeTrue();
    }
}
