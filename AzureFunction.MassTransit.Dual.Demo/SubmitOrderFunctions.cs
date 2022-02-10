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
        const string OrdersTopicName = "Orders";
        const string OrdersSubscriptionName = "Demo";
            readonly IMessageReceiver _receiver;

        public SubmitOrderFunctions(IMessageReceiver receiver)
        {
            _receiver = receiver;
        }

        [FunctionName("SubmitOrder")]
        public Task SubmitOrderAsync(

#if LOCALDEV
            [RabbitMQTrigger(OrdersTopicName, OrdersSubscriptionName)]
            BasicDeliverEventArgs message,
#else
            [ServiceBusTrigger(OrdersTopicName, OrdersSubscriptionName)]
            ServiceBusReceivedMessage message,
#endif

            CancellationToken cancellationToken)
        {
            return _receiver.HandleConsumer<SubmitOrderConsumer>(
                OrdersTopicName, OrdersSubscriptionName,
                message,
                cancellationToken);
        }
    }
}
