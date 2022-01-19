using System;
using System.Threading.Tasks;
using HeroDomain.Contracts;
using MassTransit;
using MassTransit.Context;

namespace AzureFunction.MassTransit.Demo.Consumers
{
    public class SubmitOrderConsumer : IConsumer<IOrder>
    {
        public Task Consume(ConsumeContext<IOrder> context)
        {
            LogContext.Debug?.Log("Processing Order: {OrderNumber}", context.Message.OrderNumber);

            context.Publish<OrderReceived>(new
            {
                context.Message.OrderNumber,
                Timestamp = DateTime.UtcNow
            });

            return context.RespondAsync<OrderAccepted>(new { context.Message.OrderNumber });
        }
    }
}
