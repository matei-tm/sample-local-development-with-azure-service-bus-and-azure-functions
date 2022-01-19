using System;
using Azure.Messaging.ServiceBus;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Azure.WebJobs.ServiceBus;
using Microsoft.Extensions.Logging;

namespace FunctionApp1
{
    public class AzureFunctionServiceBusQeueueDemo
    {
        [FunctionName("AzureFunctionServiceBusQeueueDemo")]
        public void Run(
            [ServiceBusTrigger("conferencesmysubscription", Connection = "AzureWebJobsServiceBus")]      
            ServiceBusReceivedMessage myQueueItem,
            ServiceBusMessageActions messageActions,
            ILogger log)
        {
            messageActions.CompleteMessageAsync(myQueueItem);
            log.LogInformation($"C# ServiceBus queue trigger function processed message: {myQueueItem}");
        }
    }
}
