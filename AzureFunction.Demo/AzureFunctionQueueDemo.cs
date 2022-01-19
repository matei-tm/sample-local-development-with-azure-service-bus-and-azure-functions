using System;
using Azure.Messaging.ServiceBus;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace AzureFunction.Demo
{
    public class AzureFunctionQueueDemo
    {
        [FunctionName("AzureFunctionQueueDemo")]
        public void Run([QueueTrigger("conferencesmysubscription", Connection = "AzureWebJobsStorage")] string myQueueItem, ILogger log)
        {
            log.LogInformation($"C# Queue trigger function processed: {myQueueItem}");
        }
    }
}
