namespace Ordering.Application.Interfaces;

/// <summary>
/// Basket servisine SENKRON çağrı soyutlaması. Checkout sırasında kullanıcının
/// sepetini Basket'ten çekmek için kullanılır (Basket'in Catalog'a gitmesiyle aynı kalıp).
/// </summary>
public interface IBasketClient
{
    Task<BasketInfo?> GetBasketAsync(string buyerId, string accessToken);
}
public record BasketInfo(List<BasketLine> Items);
public record BasketLine(int ProductId, string ProductName, decimal Price, int Quantity);