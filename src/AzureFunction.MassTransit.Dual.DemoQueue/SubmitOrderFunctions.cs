
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using AzureFunction.MassTransit.Demo.Consumers;


#if LOCALDEV
using MassTransit.WebJobs.RabbitMqIntegration;
using RabbitMQ.Client.Events;
#else
using MassTransit.WebJobs.ServiceBusIntegration;
using Microsoft.Azure.ServiceBus;
#endif

namespace AzureFunction.MassTransit.Demo
{
    public class SubmitOrderFunctions
    {
        const string OrdersQueueName = "OrdersQueue";
        readonly IMessageReceiver _receiver;

        public SubmitOrderFunctions(IMessageReceiver receiver)
        {
            _receiver = receiver;
        }

        [FunctionName("SubmitOrder")]
        public Task SubmitOrderAsync(

#if LOCALDEV
            [RabbitMQTrigger(OrdersQueueName, ConnectionStringSetting = "RabbitMQ")]
            BasicDeliverEventArgs message,
#else
            [ServiceBusTrigger(OrdersQueueName, Connection = "AzureWebJobsServiceBus")]
            Message message,
#endif

            CancellationToken cancellationToken)
        {
            return _receiver.HandleConsumer<SubmitOrderConsumer>(
                OrdersQueueName,
                message,
                cancellationToken);
        }
    }
}
