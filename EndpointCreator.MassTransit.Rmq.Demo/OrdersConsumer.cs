using MassTransit;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using HeroDomain.Contracts;

namespace EndpointCreator.Mt2Rmq.Demo
{
    public class Orders_DemoConsumer :
    IConsumer<IOrder>
    {
        readonly ILogger<Orders_DemoConsumer> _logger;

        public Orders_DemoConsumer(ILogger<Orders_DemoConsumer> logger)
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
