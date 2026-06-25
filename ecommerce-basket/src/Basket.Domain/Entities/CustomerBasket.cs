namespace Basket.Domain.Entities;

/// <summary>
/// Bir kullanıcının sepeti. Redis'te "basket:{BuyerId}" anahtarında JSON olarak tutulur.
/// İlişkisel bir varlık olmadığı için BaseEntity'den türemez.
/// </summary>
public class CustomerBasket
{
    public string BuyerId { get; set; } = string.Empty;
    public List<BasketItem> Items { get; set; } = new();

    // Hesaplanan alan; toplam tutar.
    public decimal Total => Items.Sum(i => i.Price * i.Quantity);
}
