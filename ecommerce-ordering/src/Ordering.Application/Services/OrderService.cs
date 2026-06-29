using ECommerce.Shared.Contracts;
using FluentValidation;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Ordering.Application.Common;
using Ordering.Application.DTOs;
using Ordering.Application.Interfaces;
using Ordering.Application.Mapping;
using Ordering.Domain.Entities;
using Ordering.Domain.Interfaces;

namespace Ordering.Application.Services;

public class OrderService : IOrderService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly IValidator<CreateOrderRequest> _validator;
    private readonly IBasketClient _basketClient;

    public OrderService(
        IUnitOfWork unitOfWork,
        IPublishEndpoint publishEndpoint,
        IValidator<CreateOrderRequest> validator,
        IBasketClient basketClient)
    {
        _unitOfWork = unitOfWork;
        _publishEndpoint = publishEndpoint;
        _validator = validator;
        _basketClient = basketClient;
    }

    public async Task<Result<OrderDto>> CheckoutAsync(string buyerId, string accessToken)
    {
        var basket = await _basketClient.GetBasketAsync(buyerId, accessToken);
        if (basket is null || basket.Items.Count == 0)
            return Result<OrderDto>.Failure("Sepet boş ya da bulunamadı; sipariş oluşturulamaz.");
 
        var order = new Order
        {
            BuyerId = buyerId,
            Status = OrderStatus.Pending,
            Items = basket.Items.Select(i => new OrderItem
            {
                ProductId = i.ProductId,
                ProductName = i.ProductName,
                UnitPrice = i.Price,   // fiyat SEPETTEN (Catalog kaynaklı), istemciden değil
                Quantity = i.Quantity
            }).ToList()
        };
 
        return await SaveAndPublishAsync(order, buyerId);
    }

    private async Task<Result<OrderDto>> SaveAndPublishAsync(Order order, string buyerId)
    {
         await _unitOfWork.Repository<Order>().AddAsync(order);
 
        await _publishEndpoint.Publish(new OrderSubmitted(
            order.OrderNumber,
            buyerId,
            order.Total,
            DateTime.UtcNow,
            order.Items.Select(i => new OrderSubmittedItem(i.ProductId, i.ProductName, i.UnitPrice, i.Quantity)).ToList()));
 
        await _unitOfWork.SaveChangesAsync();
 
        return Result<OrderDto>.Success(order.ToDto());
    }

    public async Task<Result<OrderDto>> CreateOrderAsync(string buyerId, CreateOrderRequest request)
    {
        var validation = await _validator.ValidateAsync(request);
        if (!validation.IsValid)
            return Result<OrderDto>.Failure(validation.Errors.Select(e => e.ErrorMessage));

        var order = new Order
        {
            BuyerId = buyerId,
            Status = OrderStatus.Pending,
            Items = request.Items.Select(i => new OrderItem
            {
                ProductId = i.ProductId,
                ProductName = i.ProductName,
                UnitPrice = i.UnitPrice,
                Quantity = i.Quantity
            }).ToList()
        };

        await _unitOfWork.Repository<Order>().AddAsync(order);

        // Olayı OUTBOX üzerinden yayınlıyoruz: Publish çağrısı, aynı DbContext
        // transaction'ında outbox tablosuna yazılır; SaveChanges ile sipariş ve
        // olay ATOMİK olarak kaydedilir, sonra arka planda RabbitMQ'ya iletilir.
        await _publishEndpoint.Publish(new OrderSubmitted(
            order.OrderNumber,
            buyerId,
            order.Total,
            DateTime.UtcNow,
            order.Items.Select(i => new OrderSubmittedItem(i.ProductId, i.ProductName, i.UnitPrice, i.Quantity)).ToList()));

        await _unitOfWork.SaveChangesAsync();

        return Result<OrderDto>.Success(order.ToDto());
    }

    public async Task<Result<OrderDto>> GetByIdAsync(int id)
    {
        var order = await _unitOfWork.Repository<Order>()
            .Query().Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == id);

        return order is null
            ? Result<OrderDto>.Failure($"Sipariş bulunamadı (Id: {id}).")
            : Result<OrderDto>.Success(order.ToDto());
    }

    public async Task<Result<IReadOnlyList<OrderDto>>> GetMyOrdersAsync(string buyerId)
    {
        var orders = await _unitOfWork.Repository<Order>()
            .Query().Include(o => o.Items)
            .Where(o => o.BuyerId == buyerId)
            .ToListAsync();

        IReadOnlyList<OrderDto> dtos = orders.Select(o => o.ToDto()).ToList();
        return Result<IReadOnlyList<OrderDto>>.Success(dtos);
    }

}
