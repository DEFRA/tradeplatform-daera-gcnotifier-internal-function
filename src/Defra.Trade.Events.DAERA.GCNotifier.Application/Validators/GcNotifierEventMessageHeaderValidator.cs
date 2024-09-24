// Copyright DEFRA (c). All rights reserved.
// Licensed under the Open Government License v3.0.

using Defra.Trade.Common.Functions.Models;
using Defra.Trade.Common.Functions.Models.Enum;
using Defra.Trade.Common.Functions.Validation;
using Defra.Trade.Events.DAERA.GCNotifier.Application.Models;
using FluentValidation;

namespace Defra.Trade.Events.DAERA.GCNotifier.Application.Validators;

public sealed class GcNotifierEventMessageHeaderValidator : AbstractValidator<TradeEventMessageHeader>
{
    private const string EqualField = "{PropertyName} must be {ComparisonValue}";
    private const string GuidField = "{PropertyName} is not a valid guid";

    public GcNotifierEventMessageHeaderValidator()
    {
        RuleFor(m => m.MessageId)
            .Cascade(CascadeMode.Stop)
            .NotNull().WithMessage(ValidationMessages.NullField)
            .Must(BeAGuid).WithMessage(GuidField);
        RuleFor(m => m.CorrelationId)
            .Cascade(CascadeMode.Stop)
            .NotNull().WithMessage(ValidationMessages.NullField)
            .NotEmpty().WithMessage(ValidationMessages.EmptyField)
            .Must(BeAGuid).WithMessage(GuidField);
        RuleFor(x => x.CausationId)
            .Cascade(CascadeMode.Stop)
            .NotNull().WithMessage(ValidationMessages.NullField)
            .NotEmpty().WithMessage(ValidationMessages.EmptyField);
        RuleFor(m => m.ContentType)
            .Equal(RemosGCNotifierConstants.ContentType, StringComparer.OrdinalIgnoreCase).WithMessage(EqualField);
        RuleFor(m => m.EntityKey)
            .Cascade(CascadeMode.Stop)
            .NotNull().WithMessage(ValidationMessages.NullField)
            .NotEmpty().WithMessage(GuidField);
        RuleFor(m => m.PublisherId)
            .Equal(RemosGCNotifierConstants.PublisherId, StringComparer.OrdinalIgnoreCase).WithMessage(EqualField);

        RuleFor(m => m.SchemaVersion)
            .Equal(RemosGCNotifierConstants.SchemaVersion).WithMessage(EqualField);
        RuleFor(m => m.Type)
            .Equal(EventType.Internal).WithMessage(EqualField);

        RuleFor(m => m.Label)
            .Equal(RemosGCNotifierConstants.Label).WithMessage(EqualField);
        RuleFor(m => m.TimestampUtc)
            .GreaterThan(0).WithMessage(ValidationMessages.NullField);
    }

    // Only validate it is a guid if it is not null. Null checks are handled by .NotNull()
    private static bool BeAGuid(string? value) => value is null || Guid.TryParse(value, out _);
}
