namespace Basket.Application.Interfaces;

/// <summary>
/// Catalog servisine SENKRON çağrı soyutlaması. Sepete ürün eklerken
/// ürünün varlığını ve güncel fiyatını doğrulamak için kullanılır.
/// </summary>
public interface ICatalogClient
{
    Task<ProductInfo?> GetProductAsync(int productId);
}

public record ProductInfo(int Id, string Name, decimal Price, int StockQuantity);
