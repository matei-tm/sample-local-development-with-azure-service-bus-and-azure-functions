using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MassTransit;
using HeroDomain.Contracts;
using Publisher.MassTransit.Demo.Core;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Configuration;

namespace Publisher.Mt2Asb.Demo
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
                    services.AddHostedService<MassTransitConsoleHostedService>();

                    services.AddHostedService<Worker>();

                    services.AddApplicationInsightsTelemetryWorkerService();
                });

        private static IBusControl ConfigureBus(IBusRegistrationContext context)
        {
            AppConfig = context.GetRequiredService<IOptions<AppConfig>>().Value;

            return Bus.Factory.CreateUsingAzureServiceBus((cfg) =>
            {
                cfg.Host(AppConfig.ServiceBusConnectionString);

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
        }
    }
}

