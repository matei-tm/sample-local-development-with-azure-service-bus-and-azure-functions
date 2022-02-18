using AzureFunction.MassTransit.Demo;
using AzureFunction.MassTransit.Demo.Consumers;
using MassTransit;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(Startup))]

namespace AzureFunction.MassTransit.Demo
{
    public class Startup :
            FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services
                .AddScoped<SubmitOrderFunctions>()
                .AddMassTransitForAzureFunctions(
                cfg =>
                {
                    cfg.AddConsumersFromNamespaceContaining<ConsumerNamespace>();
                });
        }
    }
}
