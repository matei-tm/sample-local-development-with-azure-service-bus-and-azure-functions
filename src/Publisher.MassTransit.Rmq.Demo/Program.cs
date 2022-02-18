
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MassTransit;
using Publisher.MassTransit.Demo.Core;
using HeroDomain.Contracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Publisher.Mt2Rmq.Demo
{
    public class Program
    {
        public static AppConfig AppConfig { get; set; }

        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    var devEnvironmentVariable = hostingContext.HostingEnvironment.EnvironmentName;
                    var isDevelopment = string.IsNullOrEmpty(devEnvironmentVariable) ||
                                devEnvironmentVariable.ToLower() == "development";

                    config.AddJsonFile("appsettings.json", optional: true);
                    config.AddJsonFile($"appsettings.{devEnvironmentVariable}.json", optional: true);
                    config.AddEnvironmentVariables();

                    if (isDevelopment)
                    {
                        config.AddUserSecrets<Program>();
                    }

                    if (args != null)
                        config.AddCommandLine(args);
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.Configure<AppConfig>(hostContext.Configuration.GetSection("AppConfig"));

                    services.AddMassTransit(x =>
                    {
                        x.AddBus(ConfigureBus);
                    });
                    services.AddMassTransitHostedService();

                    services.AddHostedService<Worker>();

                    services.AddApplicationInsightsTelemetryWorkerService();
                });

        private static IBusControl ConfigureBus(IBusRegistrationContext context)
        {
            AppConfig = context.GetRequiredService<IOptions<AppConfig>>().Value;

            return Bus.Factory.CreateUsingRabbitMq((cfg) =>
            {
                cfg.Host(AppConfig.Host, AppConfig.VirtualHost,
                    h =>
                    {
                        h.Username(AppConfig.Username);
                        h.Password(AppConfig.Password);
                    }
                );

                cfg.ConfigureEndpoints(context);

                var queueEntityName = "OrdersQueue";
                cfg.OverrideDefaultBusEndpointQueueName(queueEntityName);
                cfg.Durable = true;
                cfg.AutoDelete = false;
                var topicEntityName = "OrdersTopic";
                cfg.Message<IOrder>(configTopology =>
                {
                    configTopology.SetEntityName(topicEntityName);
                    var a = configTopology.EntityNameFormatter;
                });
            });
        }
    }
}
