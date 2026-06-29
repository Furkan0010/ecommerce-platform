using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ordering.Application.Interfaces;
using Ordering.Domain.Interfaces;
using Ordering.Infrastructure.Clients;
using Ordering.Infrastructure.Consumers;
using Ordering.Infrastructure.Persistence;
using Ordering.Infrastructure.Repositories;
 
namespace Ordering.Infrastructure;
 
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, IConfiguration configuration)
    {
        // --- Veritabanı ---
        services.AddDbContext<OrderingDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("OrderingDb")));
 
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddHostedService<DbInitializer>();
 
        // --- Basket'e senkron çağrı: typed HttpClient + Polly dayanıklılık ---
        services.AddHttpClient<IBasketClient, BasketClient>(client =>
        {
            client.BaseAddress = new Uri(configuration["Basket:BaseUrl"]!);
        })
        .AddStandardResilienceHandler();
 
        // --- MassTransit + RabbitMQ + Outbox ---
        services.AddMassTransit(x =>
        {
            x.AddConsumer<OrderSubmittedConsumer>();
 
            x.AddEntityFrameworkOutbox<OrderingDbContext>(o =>
            {
                o.UseSqlServer();
                o.UseBusOutbox();
            });
 
            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(
                    configuration["RabbitMq:Host"] ?? "localhost",
                    configuration["RabbitMq:VirtualHost"] ?? "/",
                    h =>
                    {
                        h.Username(configuration["RabbitMq:Username"] ?? "guest");
                        h.Password(configuration["RabbitMq:Password"] ?? "guest");
                    });
 
                cfg.ConfigureEndpoints(context);
            });
        });
 
        return services;
    }
}