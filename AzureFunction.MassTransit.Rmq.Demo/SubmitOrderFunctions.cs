using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using AzureFunction.MassTransit.Demo.Consumers;

using MassTransit.WebJobs.RabbitMqIntegration;
using RabbitMQ.Client.Events;


namespace AzureFunction.MassTransit.Demo
{
    public class SubmitOrderFunctions
    {
        const string OrdersTopicName = "Message";
        readonly IMessageReceiver _receiver;

        public SubmitOrderFunctions(IMessageReceiver receiver)
        {
            _receiver = receiver;
        }

        [FunctionName("SubmitOrder")]
        public Task SubmitOrderAsync(
            [RabbitMQTrigger(OrdersTopicName)]
            BasicDeliverEventArgs message,
            CancellationToken cancellationToken)
        {
            return _receiver.HandleConsumer<SubmitOrderConsumer>(
                OrdersTopicName, 
                message, 
                cancellationToken);
        }
    }
}
