using MassTransit;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using HeroDomain.Contracts;

namespace EndpointCreator.Mt2Rmq.Demo
{
    public class OrdersTopicConsumer :
    IConsumer<IOrder>
    {
        protected readonly ILogger<OrdersTopicConsumer> _logger;

        public OrdersTopicConsumer(ILogger<OrdersTopicConsumer> logger)
        {
            _logger = logger;
        }

        public Task Consume(ConsumeContext<IOrder> context)
        {
            _logger.LogInformation("Received Text: {Text}", context.Message.OrderId);

            return Task.CompletedTask;
        }

    }
    public class OrdersQueueConsumer :
     IConsumer<IOrder>
    {
        protected readonly ILogger<OrdersQueueConsumer> _logger;

        public OrdersQueueConsumer(ILogger<OrdersQueueConsumer> logger)
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
