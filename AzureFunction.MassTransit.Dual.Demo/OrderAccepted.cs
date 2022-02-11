using System;

namespace AzureFunction.MassTransit.Demo
{
    public interface OrderAccepted
    {
        Guid OrderId { get; }
        string OrderNumber { get; }
    }
}
