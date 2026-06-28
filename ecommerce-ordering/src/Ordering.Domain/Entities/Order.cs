using Ordering.Domain.Common;

namespace Ordering.Domain.Entities;

public class Order : BaseEntity
{
    // Servisler arası korelasyon kimliği (saga bunu kullanacak). int PK'den ayrı.
    public Guid OrderNumber { get; set; } = Guid.NewGuid();

    public string BuyerId { get; set; } = string.Empty;
    public OrderStatus Status { get; set; } = OrderStatus.Pending;

    public List<OrderItem> Items { get; set; } = new();

    public decimal Total => Items.Sum(i => i.UnitPrice * i.Quantity);
}
