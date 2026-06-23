using Catalog.Application.DTOs;
using Catalog.Domain.Entities;

namespace Catalog.Application.Mapping;

/// <summary>
/// Manuel mapping. AutoMapper artık ticari lisans gerektirdiği için
/// (ve küçük modellerde manuel mapping gayet okunaklı olduğu için) elle yazıyoruz.
/// </summary>
public static class MappingExtensions
{
    public static ProductDto ToDto(this Product p) => new(
        p.Id, p.Name, p.Description, p.Price, p.StockQuantity, p.CategoryId, p.Category?.Name);

    public static CategoryDto ToDto(this Category c) => new(
        c.Id, c.Name, c.Description);
}
