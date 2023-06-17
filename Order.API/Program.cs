using MassTransit;
using Microsoft.EntityFrameworkCore;
using Order.API.Consumers;
using Order.API.Models;
using Shared;

var builder = WebApplication.CreateBuilder(args);
ConfigurationManager configuration = builder.Configuration;
// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<AppDbContext>(
    options =>
    {
        options.UseSqlServer(configuration.GetConnectionString("SqlCon"));
    });

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<OrderRequestCompletedEventConsumer>();
  

    x.UsingRabbitMq((context, config)=>
    {
        config.Host(configuration.GetConnectionString("RabbitMQ"));
        config.ReceiveEndpoint(RabbitMQSettingsConst.OrderRequestCompletedEventtQueueName, e =>
        {
            e.ConfigureConsumer<OrderRequestCompletedEventConsumer>(context);
        });
    
 
    });
});


var app = builder.Build();

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
