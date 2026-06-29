using Ordering.Domain.Entities;

namespace Ordering.Application.DTOs;

public record OrderItemDto(int ProductId, string ProductName, decimal UnitPrice, int Quantity);

public record OrderDto(
    int Id,
    Guid OrderNumber,
    string BuyerId,
    OrderStatus Status,
    decimal Total,
    List<OrderItemDto> Items);

// İlk adımda sipariş kalemlerini istemci gönderiyor. Sonraki adımda
// "checkout" akışında bunları Basket servisinden çekeceğiz.
public record CreateOrderItem(int ProductId, string ProductName, decimal UnitPrice, int Quantity);

public record CreateOrderRequest(List<CreateOrderItem> Items);
