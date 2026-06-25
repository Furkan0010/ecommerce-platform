using Basket.Application.Interfaces;
using Basket.Domain.Interfaces;
using Basket.Infrastructure.Clients;
using Basket.Infrastructure.Persistence;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace Basket.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, IConfiguration configuration)
    {
        // --- Redis ---
        var redisConnection = configuration["Redis:Connection"]!;
        services.AddSingleton<IConnectionMultiplexer>(
            _ => ConnectionMultiplexer.Connect(redisConnection));
        services.AddScoped<IBasketRepository, RedisBasketRepository>();

        // --- Catalog'a senkron çağrı: typed HttpClient + Polly dayanıklılık ---
        services.AddHttpClient<ICatalogClient, CatalogClient>(client =>
        {
            client.BaseAddress = new Uri(configuration["Catalog:BaseUrl"]!);
        })
        .AddStandardResilienceHandler(); // retry + circuit breaker + timeout (Polly v8)

        return services;
    }
}
