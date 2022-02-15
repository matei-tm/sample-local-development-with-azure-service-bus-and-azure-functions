using System;

namespace HeroDomain.Contracts
{
    public interface IOrder
    {
        Guid OrderId { get; set; }
        string OrderNumber { get; set; }
    }
}
