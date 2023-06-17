using MassTransit;
using Microsoft.EntityFrameworkCore;
using SagaStateMachineWorkerState;
using SagaStateMachineWorkerState.StateMachine;
using Shared;
using System.Reflection;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext,services) =>
    {
       

        services.AddMassTransit(cfg =>
        {
       
            cfg.AddSagaStateMachine<OrderStateMachine, OrderStateInstance>().EntityFrameworkRepository(opt =>
            {
                opt.AddDbContext<DbContext, OrderStateDbContext>((provider, builder) =>
                {
                    builder.UseSqlServer(hostContext.Configuration.GetConnectionString("SqlCon"), m =>
                    {
                        m.MigrationsAssembly(Assembly.GetExecutingAssembly().GetName().Name);
                    });
                });
            });
    
            cfg.AddBus(provider => Bus.Factory.CreateUsingRabbitMq(factory =>
            {
                factory.Host(hostContext.Configuration.GetConnectionString("RabbitMQ"));
                factory.ReceiveEndpoint(RabbitMQSettingsConst.OrderSaga, e =>
                {

                    e.ConfigureSaga<OrderStateInstance>(provider);
                });
            }));
        });
        
        services.AddHostedService<Worker>();
    })
    .Build();

await host.RunAsync();
