
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MassTransit;
using HeroDomain.Contracts;
using Publisher.MassTransit.Demo.Core;

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
                            cfg.ConfigureEndpoints(context);
                            cfg.Host("Endpoint=sb://dmsb01.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=QKaf4xsgV2jvzKjyURf7x84ecy/wuUU2LOvVH36QzJ4=");

                            //cfg.Host("Endpoint=sb://dmsb01.servicebus.windows.net/;SharedAccessKeyName=submitorder;SharedAccessKey=31cWBfSubh0wxLgVSBAs/SY3PbG3i0m3kmQGk4zHz0I=;EntityPath=submit-order");
                            var errorHandlingDemoTopic = "orders";

                            cfg.Message<IOrder>(configTopology => configTopology.SetEntityName(errorHandlingDemoTopic));
                        });
                    });
                    services.AddMassTransitHostedService();

                    services.AddHostedService<Worker>();
                });
    }
}
