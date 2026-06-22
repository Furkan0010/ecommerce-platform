using Catalog.Domain.Common;

namespace Catalog.Domain.Entities;

public class Product : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }

    // İlişki: her ürün bir kategoriye aittir.
    public int CategoryId { get; set; }
    public Category? Category { get; set; }
}
