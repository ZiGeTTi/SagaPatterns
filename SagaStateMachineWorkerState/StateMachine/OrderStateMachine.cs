using MassTransit;
using Shared;
using Shared.Events;
using Shared.Interface;
using Shared.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace SagaStateMachineWorkerState.StateMachine
{
    public class OrderStateMachine : MassTransitStateMachine<OrderStateInstance>
    {
        public Event<IOrderCreatedRequestEvent> OrderCreatedRequestEvent { get; set; }
        public Event<IStockReservedEvent> StockReservedEvent { get; set; }
        public Event<IStockNotReservedEvent> StockNotReservedEvent { get; set; }

        public Event<IPaymentCompletedEvent> PaymentCompletedEvent { get; set; }

        public Event<IPaymentFailedEvent> PaymentFailedEvent { get; set; }
        public State OrderCreated { get; private set; }
        public State StockReserved { get; private set; }
        public State StockNotReserved { get; private set; }
        public State PaymentCompleted { get; private set; }
        public State PaymentFailed { get; private set; }

        public  OrderStateMachine()
        {
            InstanceState(x => x.CurrentState);
            Event(() => OrderCreatedRequestEvent, y=> y.CorrelateBy<int>(x=>x.OrderId, z=>z.Message.OrderId).SelectId(context=>Guid.NewGuid() ));

            Event(() => StockReservedEvent, x => x.CorrelateById(y => y.Message.CorrelationId));

            Event(() => StockNotReservedEvent, x => x.CorrelateById(y => y.Message.CorrelationId));

            Event(() => PaymentCompletedEvent, x => x.CorrelateById(y => y.Message.CorrelationId));

            Initially(
                When(OrderCreatedRequestEvent).Then(context=>
                {
                    context.Saga.BuyerId = context.Message.BuyerId;
                    context.Saga.OrderId = context.Message.OrderId;
                    context.Saga.CreatedDate = DateTime.Now;
                    context.Saga.CardName = context.Message.Payment.CardName;
                    context.Saga.CardNumber = context.Message.Payment.CardNumber;
                    context.Saga.CVV = context.Message.Payment.CVV;
                    context.Saga.Expiration = context.Message.Payment.Expiration;
                    context.Saga.TotalPrice = context.Message.Payment.TotalPrice;
                }).Then(context => { Console.WriteLine($"OrderCreatedRequestEvent before : {context.Saga}"); })
                  .Publish(context=> new OrderCreatedEvent(context.Saga.CorrelationId) { OrderItems = context.Message.OrderItems })
                  .TransitionTo(OrderCreated)
                  .Then(context => { Console.WriteLine($"OrderCreatedRequestEvent After: {context.Saga}"); }));

            During(OrderCreated, When(StockReservedEvent).TransitionTo(StockReserved)
               .Send(new  Uri($"queue:{RabbitMQSettingsConst.PaymentStockReservedRequestQueueName}"),context=>
               
               new StockReservedRequestPayment(context.Saga.CorrelationId)
               {
                   orderItems = context.Message.OrderItems,
                   payment=new PaymentMessage()
                   {
                       CardName = context.Saga.CardName,
                       CardNumber = context.Saga.CardNumber,
                       CVV = context.Saga.CVV,
                       Expiration = context.Saga.Expiration,
                       TotalPrice = context.Saga.TotalPrice
                   },
                   BuyerId = context.Saga.BuyerId
               })
               .Then(context => { Console.WriteLine($"StockReserved After:{context.Saga}"); }));
            During(StockReserved, 
                When(PaymentCompletedEvent).TransitionTo(PaymentCompleted)
                .Publish(context => new OrderRequestCompletedEvent() {OrderId = context.Saga.OrderId }).Then(context => { Console.WriteLine($"PaymentCompletedEvent After :{context.Saga}"); }).Finalize(),
                When(PaymentFailedEvent)
                .Publish(context => new OrderRequestFailedEvent() { OrderId = context.Saga.OrderId, Reason = context.Message.Reason })
                .Send(new Uri($"queue:{RabbitMQSettingsConst.StockRollBackMessageQueueName}"),context=> new StockRollBackMessage() { OrderItems = context.Message.OrderItems })
                .TransitionTo(PaymentFailed)
                .Then(context => { Console.WriteLine($"PaymentFailed After:{context.Saga}"); })
                );


            SetCompletedWhenFinalized();
        }

    }
}
