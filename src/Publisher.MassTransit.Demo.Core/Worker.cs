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
            WriteInfo();
            var userAnswer = Console.ReadKey().Key;
            while (userAnswer != ConsoleKey.X)
            {
                if (userAnswer == ConsoleKey.S)
                {
                    var s = await _bus.GetSendEndpoint(_bus.Address);
                    await s.Send(GetRandomOrder(isPublished: false));
                }
                else if (userAnswer == ConsoleKey.P)
                {
                    await _bus.Publish(GetRandomOrder(isPublished: true));
                }

                WriteInfo();
                userAnswer = Console.ReadKey().Key;
            }

        }

        private static IOrder GetRandomOrder(bool isPublished)
        {
            var randomOrderNumber = new Random().Next(0, 100).ToString();

            Console.WriteLine($" new message will be {(isPublished ? "Published to topic" : "Sent to queue")} with # {randomOrderNumber}");
            IOrder order = new Order { OrderId = Guid.NewGuid(), OrderNumber = randomOrderNumber };

            return order;
        }

        private static void WriteInfo()
        {
            Console.WriteLine("Enter the option: [S]end order to queue/[P]ublish to topic/e[X]it");
        }
    }
}
