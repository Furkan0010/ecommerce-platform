using System.Text.Json;
using Basket.Domain.Entities;
using Basket.Domain.Interfaces;
using StackExchange.Redis;

namespace Basket.Infrastructure.Persistence;

/// <summary>
/// IBasketRepository'nin Redis implementasyonu. Sepet, "basket:{buyerId}"
/// anahtarında JSON string olarak tutulur.
/// </summary>
public class RedisBasketRepository : IBasketRepository
{
    private readonly IDatabase _database;

    public RedisBasketRepository(IConnectionMultiplexer redis)
    {
        _database = redis.GetDatabase();
    }

    public async Task<CustomerBasket?> GetBasketAsync(string buyerId)
    {
        var data = await _database.StringGetAsync(BasketKey(buyerId));
        return data.IsNullOrEmpty
            ? null
            : JsonSerializer.Deserialize<CustomerBasket>(data!);
    }

    public async Task<CustomerBasket> UpdateBasketAsync(CustomerBasket basket)
    {
        await _database.StringSetAsync(BasketKey(basket.BuyerId), JsonSerializer.Serialize(basket));
        return basket;
    }

    public Task<bool> DeleteBasketAsync(string buyerId)
        => _database.KeyDeleteAsync(BasketKey(buyerId));

    private static string BasketKey(string buyerId) => $"basket:{buyerId}";
}
