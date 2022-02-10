using MassTransit;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using HeroDomain.Contracts;

namespace EndpointCreator.Mt2Rmq.Demo
{
    public class OrdersConsumer :
    IConsumer<IOrder>
    {
        readonly ILogger<OrdersConsumer> _logger;

        public OrdersConsumer(ILogger<OrdersConsumer> logger)
        {
            _logger = logger;
        }

        public Task Consume(ConsumeContext<IOrder> context)
        {
            _logger.LogInformation("Received Text: {Text}", context.Message.OrderId);

            return Task.CompletedTask;
        }

    }
}
