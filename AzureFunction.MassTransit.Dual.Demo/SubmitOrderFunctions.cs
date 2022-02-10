using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using AzureFunction.MassTransit.Demo.Consumers;

#if LOCALDEV
using MassTransit.WebJobs.RabbitMqIntegration;
using RabbitMQ.Client.Events;
#else
using MassTransit.WebJobs.ServiceBusIntegration;
using Azure.Messaging.ServiceBus;
#endif

namespace AzureFunction.MassTransit.Demo
{
    public class SubmitOrderFunctions
    {
        const string OrdersTopicName = "orders";
        readonly IMessageReceiver _receiver;

        public SubmitOrderFunctions(IMessageReceiver receiver)
        {
            _receiver = receiver;
        }

        [FunctionName("SubmitOrder")]
        public Task SubmitOrderAsync(

#if LOCALDEV
            [RabbitMQTrigger(OrdersTopicName)]
            BasicDeliverEventArgs message,
#else
            [ServiceBusTrigger(OrdersTopicName, "democonsumer")]
            ServiceBusReceivedMessage message,
#endif

            CancellationToken cancellationToken)
        {
            return _receiver.HandleConsumer<SubmitOrderConsumer>(
                OrdersTopicName, "democonsumer",
                message,
                cancellationToken);
        }
    }
}
