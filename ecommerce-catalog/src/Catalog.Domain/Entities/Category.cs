using Catalog.Domain.Common;

namespace Catalog.Domain.Entities;

public class Category : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    // Bir kategorinin birden çok ürünü olabilir (blogdaki Post-Comment ilişkisi gibi).
    public ICollection<Product> Products { get; set; } = new List<Product>();
}
