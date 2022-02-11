using System;

namespace AzureFunction.MassTransit.Demo
{
    public interface OrderReceived
    {
        Guid OrderId { get; }
        DateTime Timestamp { get; }

        string OrderNumber { get; }
    }
}
