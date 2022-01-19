
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MassTransit;
using Publisher.MassTransit.Demo.Core;

namespace Publisher.Mt2Rmq.Demo
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
                        //x.AddConsumer<MessageConsumer>();
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
}
