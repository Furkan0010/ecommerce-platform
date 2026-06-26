namespace Basket.Application.DTOs;

public record BasketItemDto(int ProductId, string ProductName, decimal Price, int Quantity);

public record BasketDto(string BuyerId, List<BasketItemDto> Items, decimal Total);

public record AddItemRequest(int ProductId, int Quantity);
