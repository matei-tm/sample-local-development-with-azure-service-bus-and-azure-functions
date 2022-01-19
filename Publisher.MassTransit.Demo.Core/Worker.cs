using System;
using System.Threading;
using System.Threading.Tasks;
using HeroDomain.Contracts;
using MassTransit;
using Microsoft.Extensions.Hosting;

namespace Publisher.MassTransit.Demo.Core
{
    public class Worker : BackgroundService
    {
        readonly IBus _bus;

        public Worker(IBus bus)
        {
            _bus = bus;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Console.WriteLine("Enter the option: [A]dd order/e[X]it");
            while (Console.ReadKey().Key == ConsoleKey.A)
            {
                var randomOrderNumber = new Random().Next(0, 100).ToString();
                Console.WriteLine($" new message will be published with # {randomOrderNumber}");
                IOrder o = new Order { OrderId = Guid.NewGuid(), OrderNumber = randomOrderNumber };
                await _bus.Publish(o);
                Console.WriteLine("Enter the option: [A]dd order/e[X]it");
            }

        }
    }
}