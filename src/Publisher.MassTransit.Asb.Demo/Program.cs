
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MassTransit;
using HeroDomain.Contracts;
using Publisher.MassTransit.Demo.Core;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Publisher.Mt2Asb.Demo
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddMassTransit(x =>
                    {
                        x.UsingAzureServiceBus((context, cfg) =>
                        {

                            cfg.Host("Endpoint=sb://dmsb01.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=/kKBvFzITS/REniOR+kWZpubzmVVj4K7PoaE548rtlU=");
                            cfg.ConfigureEndpoints(context);

                            var queueEntityName = "OrdersQueue";
                            cfg.OverrideDefaultBusEndpointQueueName(queueEntityName);

                            var topicEntityName = "OrdersTopic";
                            cfg.Message<IOrder>(configTopology =>
                            {
                                configTopology.SetEntityName(topicEntityName);
                                var a = configTopology.EntityNameFormatter;
                            });
                        });
                    });
                    services.AddMassTransitHostedService();
                    services.AddHostedService<Worker>();
                });
    }

    public class OrdersConsumer :
    IConsumer<Order>
    {
        readonly ILogger<OrdersConsumer> _logger;

        public OrdersConsumer(ILogger<OrdersConsumer> logger)
        {
            _logger = logger;
        }

        public Task Consume(ConsumeContext<Order> context)
        {
            _logger.LogInformation("Received Text: {Text}", context.Message.OrderId);

            return Task.CompletedTask;
        }

    }
}
