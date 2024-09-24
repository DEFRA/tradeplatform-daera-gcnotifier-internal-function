// Copyright DEFRA (c). All rights reserved.
// Licensed under the Open Government License v3.0.

using Defra.Trade.Events.DAERA.GCNotifier.Application.Dtos.Inbound;
using FluentValidation;

namespace Defra.Trade.Events.DAERA.GCNotifier.Application.Validators;

public class GcNotificationInboundValidator : AbstractValidator<GCNotificationInbound>
{
    public GcNotificationInboundValidator()
    {
        RuleFor(m => m.GCId)
            .NotEmpty();
    }
}