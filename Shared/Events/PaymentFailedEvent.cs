using Shared.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared
{
    public class PaymentFailedEvent:IPaymentFailedEvent
    {
        public PaymentFailedEvent(Guid correlationId)
        {
            CorrelationId = correlationId;
        }
        public List<OrderItemMessage> OrderItems { get  ; set; }
        public string Reason { get ; set ; }

        public Guid CorrelationId { get; set; }
    }
}
