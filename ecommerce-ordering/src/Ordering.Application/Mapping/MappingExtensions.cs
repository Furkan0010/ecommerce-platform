using Ordering.Application.DTOs;
using Ordering.Domain.Entities;

namespace Ordering.Application.Mapping;

public static class MappingExtensions
{
    public static OrderDto ToDto(this Order o) => new(
        o.Id,
        o.OrderNumber,
        o.BuyerId,
        o.Status,
        o.Total,
        o.Items.Select(i => new OrderItemDto(i.ProductId, i.ProductName, i.UnitPrice, i.Quantity)).ToList());
}
