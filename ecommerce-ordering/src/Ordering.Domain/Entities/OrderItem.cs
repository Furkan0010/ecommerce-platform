using Ordering.Domain.Common;

namespace Ordering.Domain.Entities;

public class OrderItem : BaseEntity
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }

    public int OrderId { get; set; }
    public Order? Order { get; set; }
}
