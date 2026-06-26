using Basket.Application.Common;
using Basket.Application.DTOs;

namespace Basket.Application.Interfaces;

public interface IBasketService
{
    Task<Result<BasketDto>> GetBasketAsync(string buyerId);
    Task<Result<BasketDto>> AddItemAsync(string buyerId, AddItemRequest request);
    Task<Result<BasketDto>> RemoveItemAsync(string buyerId, int productId);
    Task<Result> ClearAsync(string buyerId);
}
