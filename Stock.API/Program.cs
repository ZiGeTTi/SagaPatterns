using MassTransit;
using Microsoft.EntityFrameworkCore;
using Shared;
using Stock.API.Consumer;
using Stock.API.Models;

var builder = WebApplication.CreateBuilder(args);
ConfigurationManager configuration = builder.Configuration;
// Add services to the container.

builder.Services.AddDbContext<AppDbContext>(options=>
options.UseInMemoryDatabase("StockDb"));

 
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<OrderCreatedEventConsumer>();
    x.AddConsumer<StockRollBackMessageConsumer>();
    x.UsingRabbitMq((context, config) =>
    {
       
        config.Host(configuration.GetConnectionString("RabbitMQ"));
        config.ReceiveEndpoint(RabbitMQSettingsConst.StockOrderCreatedEventQueueName, e =>
        {
            e.ConfigureConsumer<OrderCreatedEventConsumer>(context);
        });

        config.ReceiveEndpoint(RabbitMQSettingsConst.StockRollBackMessageQueueName, e =>
        {
            e.ConfigureConsumer<StockRollBackMessageConsumer>(context);
        });
        });
});
var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    context.Stocks.Add(new Stock.API.Models.Stock() {  Id = 1, ProductId = 1, Count = 100 });
    context.Stocks.Add(new Stock.API.Models.Stock() {  Id = 2, ProductId = 2, Count = 200 });
    context.SaveChanges();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
