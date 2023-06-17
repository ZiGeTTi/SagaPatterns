using Shared.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Events
{
    public class StockReservedRequestPayment : IStockReservedRequestPayment
    {
        public StockReservedRequestPayment(Guid correlationId)
        {
            CorrelationId = correlationId;
        }

        public Guid CorrelationId { get; }
        public PaymentMessage payment { get; set; }
        public List<OrderItemMessage> orderItems { get; set; }

        public string BuyerId { get; set; }
    }
}
