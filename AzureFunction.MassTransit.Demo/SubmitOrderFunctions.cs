using System.Threading;
using System.Threading.Tasks;
using MassTransit.WebJobs.ServiceBusIntegration;
using Microsoft.Azure.WebJobs;
using AzureFunction.MassTransit.Demo.Consumers;
using Azure.Messaging.ServiceBus;

namespace AzureFunction.MassTransit.Demo
{
    public class SubmitOrderFunctions
    {
        const string SubmitOrderTopicName = "conferences";
        const string SubmitOrderSubscriptionName = "mysubscription";
        readonly IMessageReceiver _receiver;

        public SubmitOrderFunctions(IMessageReceiver receiver)
        {
            _receiver = receiver;
        }

        [FunctionName("SubmitOrder")]
        public Task SubmitOrderAsync(
            [ServiceBusTrigger(SubmitOrderTopicName, SubmitOrderSubscriptionName)]
            ServiceBusReceivedMessage message,
            CancellationToken cancellationToken)
        {
            return _receiver.HandleConsumer<SubmitOrderConsumer>(
                SubmitOrderTopicName,
                SubmitOrderSubscriptionName,
                message,
                cancellationToken);
        }
    }
}
