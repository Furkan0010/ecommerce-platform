namespace Basket.Domain.Entities;

public class BasketItem
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal Price { get; set; }      // Catalog'dan gelen güncel fiyat
    public int Quantity { get; set; }
}
