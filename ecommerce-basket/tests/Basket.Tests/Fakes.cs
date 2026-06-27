using Basket.Application.Interfaces;
using Basket.Domain.Entities;
using Basket.Domain.Interfaces;

namespace Basket.Tests;

/// <summary>Bellekte tutan sahte sepet deposu (Redis yerine).</summary>
public class FakeBasketRepository : IBasketRepository
{
    private readonly Dictionary<string, CustomerBasket> _store = new();

    public Task<CustomerBasket?> GetBasketAsync(string buyerId)
        => Task.FromResult(_store.TryGetValue(buyerId, out var b) ? b : null);

    public Task<CustomerBasket> UpdateBasketAsync(CustomerBasket basket)
    {
        _store[basket.BuyerId] = basket;
        return Task.FromResult(basket);
    }

    public Task<bool> DeleteBasketAsync(string buyerId)
        => Task.FromResult(_store.Remove(buyerId));
}

/// <summary>Catalog'a gitmeyen sahte istemci.</summary>
public class FakeCatalogClient : ICatalogClient
{
    private readonly ProductInfo? _product;
    public FakeCatalogClient(ProductInfo? product) => _product = product;

    public Task<ProductInfo?> GetProductAsync(int productId)
        => Task.FromResult(_product?.Id == productId ? _product : null);
}
