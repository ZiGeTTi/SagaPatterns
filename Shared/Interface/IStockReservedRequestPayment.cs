using MassTransit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Interface
{
  public   interface IStockReservedRequestPayment:CorrelatedBy<Guid>
    {
        public PaymentMessage payment{ get; set; }
        public List<OrderItemMessage> orderItems { get; set; }

        public string BuyerId { get; set; }
    }
}
