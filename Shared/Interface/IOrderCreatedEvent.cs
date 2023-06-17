using MassTransit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Interface
{
    public interface IOrderCreatedEvent : CorrelatedBy<Guid>
    {
        List<OrderItemMessage> OrderItems { get; set; }
    }
}
