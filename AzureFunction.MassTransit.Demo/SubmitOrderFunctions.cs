using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using AzureFunction.MassTransit.Demo.Consumers;

using MassTransit.WebJobs.ServiceBusIntegration;
using Azure.Messaging.ServiceBus;

namespace AzureFunction.MassTransit.Demo
{
    public class SubmitOrderFunctions
    {
        const string OrdersTopicName = "Orders";
        readonly IMessageReceiver _receiver;

        public SubmitOrderFunctions(IMessageReceiver receiver)
        {
            _receiver = receiver;
        }

        [FunctionName("SubmitOrder")]
        public Task SubmitOrderAsync(
            [ServiceBusTrigger(OrdersTopicName)]
            ServiceBusReceivedMessage message,
            CancellationToken cancellationToken)
        {
            return _receiver.HandleConsumer<SubmitOrderConsumer>(
                OrdersTopicName,
                message,
                cancellationToken);
        }
    }
}
