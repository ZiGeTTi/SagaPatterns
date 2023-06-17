using Shared.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared
{
    public class PaymentCompletedEvent:IPaymentCompletedEvent
    {
 
        public Guid CorrelationId { get; set; }

        public PaymentCompletedEvent(Guid correlationId)
        {
            CorrelationId = correlationId;
        }
    }
}
