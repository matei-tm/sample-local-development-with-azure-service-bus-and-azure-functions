using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MassTransit;
using HeroDomain.Contracts;
using Publisher.MassTransit.Demo.Core;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Configuration;
using System;

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
                    var rabbitMqEnabled = Environment.GetEnvironmentVariable("RABBITMQ_ENABLED");

                    if (rabbitMqEnabled == "true")
                    {
                        services.Configure<AppConfigRabbitMq>(hostContext.Configuration.GetSection("AppConfig-RabbitMq"));

                        services.AddMassTransit(x =>
                        {
                            x.AddBus(ConfigureBusForRabbitMq);
                        });
                    }
                    else
                    {
                        services.Configure<AppConfigServiceBus>(hostContext.Configuration.GetSection("AppConfig-AzureServiceBus"));

                        services.AddMassTransit(x =>
                        {
                            x.AddBus(ConfigureBusForAzureServiceBus);
                        });
                    }

                    services.AddHostedService<MassTransitConsoleHostedService>();

                    services.AddHostedService<Worker>();

                    services.AddApplicationInsightsTelemetryWorkerService();
                });

        private static IBusControl ConfigureBusForAzureServiceBus(IBusRegistrationContext context)
        {
            var appConfig = context.GetRequiredService<IOptions<AppConfigServiceBus>>().Value;

            return Bus.Factory.CreateUsingAzureServiceBus((cfg) =>
            {
                cfg.Host(appConfig.ServiceBusConnectionString);

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

        private static IBusControl ConfigureBusForRabbitMq(IBusRegistrationContext context)
        {
            var appConfig = context.GetRequiredService<IOptions<AppConfigRabbitMq>>().Value;

            return Bus.Factory.CreateUsingRabbitMq((cfg) =>
            {
                cfg.Host(appConfig.Host, appConfig.VirtualHost,
                    h =>
                    {
                        h.Username(appConfig.Username);
                        h.Password(appConfig.Password);
                    }
                );

                cfg.ConfigureEndpoints(context);

                var queueEntityName = "OrdersQueue";
                cfg.OverrideDefaultBusEndpointQueueName(queueEntityName);

                var topicEntityName = "OrdersTopic";
                cfg.Message<IOrder>(configTopology =>
                {
                    configTopology.SetEntityName(topicEntityName);
                    var a = configTopology.EntityNameFormatter;
                });

                cfg.Durable = true;
                cfg.AutoDelete = false;
            });
        }
    }
}

