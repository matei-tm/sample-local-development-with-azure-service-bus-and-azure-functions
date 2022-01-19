using HeroDomain.Contracts;
using System;

namespace Publisher.MassTransit.Demo.Core
{
    public class Order : IOrder
    {
        public Guid OrderId { get; set; }
        public string OrderNumber { get; set; }
    }
}
