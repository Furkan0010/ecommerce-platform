using Basket.Application.DTOs;
using Basket.Domain.Entities;

namespace Basket.Application.Mapping;

public static class MappingExtensions
{
    public static BasketDto ToDto(this CustomerBasket b) => new(
        b.BuyerId,
        b.Items.Select(i => new BasketItemDto(i.ProductId, i.ProductName, i.Price, i.Quantity)).ToList(),
        b.Total);
}
