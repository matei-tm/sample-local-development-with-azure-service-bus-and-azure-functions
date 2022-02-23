
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MassTransit;
using Publisher.MassTransit.Demo.Core;
using System.Threading;
using System.Threading.Tasks;
using System;

namespace EndpointCreator.Mt2Rmq.Demo
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
                        x.AddConsumer<OrdersTopicConsumer>();
                        x.AddConsumer<OrdersQueueConsumer>();

                        x.UsingRabbitMq((context, cfg) =>
                        {
                            cfg.Host("localhost", "/", "RabbitMQ",


                                h =>
                                {
                                    h.Username("guest");
                                    h.Password("guest");

                                }

                            );
                            cfg.ConfigureEndpoints(context);

                        });
                    });
                    services.AddMassTransitHostedService();

                    services.AddHostedService<Worker>();
                });
    }

    public class Worker : BackgroundService
    {
        private readonly IHostApplicationLifetime _host;

        public Worker(IHostApplicationLifetime host)
        {
            _host = host;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Console.WriteLine("Give RabbitMq a chance to create demo queues. The application will close in 5 seconds");

            await Task.Delay(5000, stoppingToken);

            Console.WriteLine("The RabbitMq demo setup is finished. Nothing else to process.");

            _host.StopApplication();
            await Task.CompletedTask;
        }
    }
}
