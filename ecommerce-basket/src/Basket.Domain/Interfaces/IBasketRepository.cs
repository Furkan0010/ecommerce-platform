using Basket.Domain.Entities;

namespace Basket.Domain.Interfaces;

/// <summary>
/// Sepetin saklandığı yerin soyutlaması. Implementasyonu Redis (Infrastructure).
/// </summary>
public interface IBasketRepository
{
    Task<CustomerBasket?> GetBasketAsync(string buyerId);
    Task<CustomerBasket> UpdateBasketAsync(CustomerBasket basket);
    Task<bool> DeleteBasketAsync(string buyerId);
}
