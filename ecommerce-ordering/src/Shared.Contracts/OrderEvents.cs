namespace ECommerce.Shared.Contracts;

/// <summary>
/// Bir sipariş oluşturulduğunda yayınlanan olay. Tüm servisler bu sözleşmeyi
/// (aynı namespace + tip adıyla) paylaşır; MassTransit mesajları buna göre eşler.
/// </summary>
public record OrderSubmitted(
    Guid OrderId,
    string BuyerId,
    decimal Total,
    DateTime SubmittedAt,
    IReadOnlyList<OrderSubmittedItem> Items);

public record OrderSubmittedItem(
    int ProductId,
    string ProductName,
    decimal UnitPrice,
    int Quantity);
