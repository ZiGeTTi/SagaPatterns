using MassTransit;
using Microsoft.EntityFrameworkCore;
using Shared;
using Shared.Events;
using Shared.Interface;
using Stock.API.Models;

namespace Stock.API.Consumer
{
    public class OrderCreatedEventConsumer : IConsumer<IOrderCreatedEvent>
    {
        private readonly AppDbContext _appContext;
        private ILogger<OrderCreatedEventConsumer> _logger;
        private readonly ISendEndpointProvider _sendEndpointProvider;
        private readonly IPublishEndpoint _publishEndpoint;

        public OrderCreatedEventConsumer(AppDbContext appContext, ILogger<OrderCreatedEventConsumer> logger, ISendEndpointProvider sendEndpointProvider, IPublishEndpoint publishEndpoint)
        {
            _appContext = appContext;
            _logger = logger;
            _sendEndpointProvider = sendEndpointProvider;
            _publishEndpoint = publishEndpoint;
        }

        public async Task Consume(ConsumeContext<IOrderCreatedEvent> context)
        {
            var stockResult = new List<bool>();
            foreach (var item in context.Message.OrderItems)
            {
                stockResult.Add(await _appContext.Stocks.AnyAsync(x=>x.ProductId == item.ProductId && x.Count > item.Count));
            }
            if (stockResult.All(x=>x.Equals(true)))
            {
                foreach (var item in context.Message.OrderItems)
                {
                    var stock = await _appContext.Stocks.FirstOrDefaultAsync(x=>x.ProductId == item.ProductId);
                    if (stock!=null)
                    {
                        stock.Count -= item.Count;
                    }

                    await _appContext.SaveChangesAsync();
                }
                _logger.LogInformation($"Stock was reseverved for CorrelationId:{context.Message.CorrelationId}");

                var sendEndpoint = await _sendEndpointProvider.GetSendEndpoint(new Uri($"queue:{RabbitMQSettingsConst.StockReservedEventQueueName}"));

                StockReservedEvent stockNotReservedEvent = new StockReservedEvent(context.Message.CorrelationId)
                {
                
                    OrderItems = context.Message.OrderItems
                };

                await sendEndpoint.Send(stockNotReservedEvent);
            }
            else
            {
                await _publishEndpoint.Publish(new StockNotReservedEvent(context.Message.CorrelationId)
                {
                  Reason = "Not enough stock"
                });

                _logger.LogInformation($"Not enough stock for Buyer ID :{context.Message.CorrelationId}");
            }

        }
    }
}
