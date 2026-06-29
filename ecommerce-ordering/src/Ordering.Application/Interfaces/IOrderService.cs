using Ordering.Application.Common;
using Ordering.Application.DTOs;

namespace Ordering.Application.Interfaces;

public interface IOrderService
{
    Task<Result<OrderDto>> CreateOrderAsync(string buyerId, CreateOrderRequest request);
    Task<Result<OrderDto>> GetByIdAsync(int id);
    Task<Result<IReadOnlyList<OrderDto>>> GetMyOrdersAsync(string buyerId);
    Task<Result<OrderDto>> CheckoutAsync(string buyerId, string accessToken);

}
