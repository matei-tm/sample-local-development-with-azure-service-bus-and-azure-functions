using MassTransit;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using HeroDomain.Contracts;

namespace Publisher.Mt2Rmq.Demo
{
    public class MessageConsumer :
    IConsumer<IOrder>
    {
        readonly ILogger<MessageConsumer> _logger;

        public MessageConsumer(ILogger<MessageConsumer> logger)
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
