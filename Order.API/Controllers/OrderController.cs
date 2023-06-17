using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Order.API.Dtos;
using Order.API.Models;
using Shared;
using Shared.Events;
using Shared.Interface;

namespace Order.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly AppDbContext _context;

        private readonly ISendEndpointProvider _sendEndpointProvider;

        public OrderController(AppDbContext context, IPublishEndpoint publishEndpoint, ISendEndpointProvider sendEndpointProvider)
        {
            _context = context;
            _sendEndpointProvider = sendEndpointProvider;
        }

        [HttpPost]
        public async Task<IActionResult> Create(OrderCreateDto orderCreate)
        {
            var newOrder = new Models.Order
            {
                BuyerId = orderCreate.BuyerId,
                Status = OrderStatus.Suspend,
                Address = new Address { Line = orderCreate.Address.Line, Province = orderCreate.Address.Province, District = orderCreate.Address.District },
                CreatedDate = DateTime.Now
            };

            orderCreate.OrderItems.ForEach(item =>
            {
                newOrder.Items.Add(new OrderItem() { Price = item.Price, ProductId = item.ProductId, Count = item.Count });
            });

       var aa =     await _context.AddAsync(newOrder);

            await _context.SaveChangesAsync();

            var orderCreatedRequestEvent = new OrderCreatedRequestEvent()
            {
                BuyerId = orderCreate.BuyerId,
                OrderId = newOrder.Id,
                Payment = new PaymentMessage
                {
                    CardName = orderCreate.payment.CardName,
                    CardNumber = orderCreate.payment.CardNumber,
                    Expiration = orderCreate.payment.Expiration,
                    CVV = orderCreate.payment.CVV,
                    TotalPrice = orderCreate.OrderItems.Sum(x => x.Price * x.Count)
                },
            };

            orderCreate.OrderItems.ForEach(item =>
            {
                orderCreatedRequestEvent.OrderItems.Add(new OrderItemMessage { Count = item.Count, ProductId = item.ProductId });
            });
            var sendEndpoint = await _sendEndpointProvider.GetSendEndpoint(new Uri($"queue:{RabbitMQSettingsConst.OrderSaga}"));
            await sendEndpoint.Send<IOrderCreatedRequestEvent>(orderCreatedRequestEvent);

            return Ok();
        }
    }
}
