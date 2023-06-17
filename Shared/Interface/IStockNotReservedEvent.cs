using MassTransit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Interface
{
   


    public interface IStockReservedEvent : CorrelatedBy<Guid>
    {
        List<OrderItemMessage> OrderItems { get; set; }
    }
    public interface IStockNotReservedEvent : CorrelatedBy<Guid>
    {
        string Reason { get; set; }
    }
}
