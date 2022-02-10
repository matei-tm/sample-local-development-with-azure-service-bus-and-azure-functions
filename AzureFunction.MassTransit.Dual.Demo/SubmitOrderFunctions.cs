using System.Threading;
using System.Threading.Tasks;

using Microsoft.Azure.WebJobs;
using AzureFunction.MassTransit.Demo.Consumers;
using HeroDomain.Contracts;
using MassTransit.WebJobs.RabbitMqIntegration;
using RabbitMQ.Client.Events;


namespace AzureFunction.MassTransit.Demo
{
    public class SubmitOrderFunctions
    {
        const string SubmitOrderTopicName = "Message";
        readonly IMessageReceiver _receiver;

        public SubmitOrderFunctions(IMessageReceiver receiver)
        {
            _receiver = receiver;
        }

        [FunctionName("SubmitOrder")]
        public Task SubmitOrderAsync(
            [RabbitMQTrigger(SubmitOrderTopicName)]
            BasicDeliverEventArgs message,
            CancellationToken cancellationToken)
        {
            return _receiver.HandleConsumer<SubmitOrderConsumer>(SubmitOrderTopicName, message, cancellationToken);
        }
    }
}
