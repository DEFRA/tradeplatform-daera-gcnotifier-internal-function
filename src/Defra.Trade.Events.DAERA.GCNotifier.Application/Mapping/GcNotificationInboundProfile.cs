// Copyright DEFRA (c). All rights reserved.
// Licensed under the Open Government License v3.0.

using AutoMapper;
using Defra.Trade.Events.DAERA.GCNotifier.Application.Dtos.Inbound;

namespace Defra.Trade.Events.DAERA.GCNotifier.Application.Mapping;

public sealed class GcNotificationInboundProfile : Profile
{
    public GcNotificationInboundProfile()
    {
        CreateMap<GCNotificationInbound, GcNotificationRequest>();
    }
}