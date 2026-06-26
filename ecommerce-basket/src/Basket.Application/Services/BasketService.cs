using Basket.Application.Common;
using Basket.Application.DTOs;
using Basket.Application.Interfaces;
using Basket.Application.Mapping;
using Basket.Domain.Entities;
using Basket.Domain.Interfaces;
using FluentValidation;

namespace Basket.Application.Services;

public class BasketService : IBasketService
{
    private readonly IBasketRepository _repository;
    private readonly ICatalogClient _catalogClient;
    private readonly IValidator<AddItemRequest> _addItemValidator;

    public BasketService(
        IBasketRepository repository,
        ICatalogClient catalogClient,
        IValidator<AddItemRequest> addItemValidator)
    {
        _repository = repository;
        _catalogClient = catalogClient;
        _addItemValidator = addItemValidator;
    }

    public async Task<Result<BasketDto>> GetBasketAsync(string buyerId)
    {
        var basket = await _repository.GetBasketAsync(buyerId)
                     ?? new CustomerBasket { BuyerId = buyerId };
        return Result<BasketDto>.Success(basket.ToDto());
    }

    public async Task<Result<BasketDto>> AddItemAsync(string buyerId, AddItemRequest request)
    {
        var validation = await _addItemValidator.ValidateAsync(request);
        if (!validation.IsValid)
            return Result<BasketDto>.Failure(validation.Errors.Select(e => e.ErrorMessage));

        // --- SENKRON SERVİS ÇAĞRISI: ürünü Catalog'dan doğrula ---
        var product = await _catalogClient.GetProductAsync(request.ProductId);
        if (product is null)
            return Result<BasketDto>.Failure($"Ürün bulunamadı (Id: {request.ProductId}).");

        if (product.StockQuantity < request.Quantity)
            return Result<BasketDto>.Failure("Yeterli stok yok.");

        var basket = await _repository.GetBasketAsync(buyerId)
                     ?? new CustomerBasket { BuyerId = buyerId };

        var existing = basket.Items.FirstOrDefault(i => i.ProductId == product.Id);
        if (existing is null)
        {
            // Fiyatı ve adı istemciden DEĞİL, Catalog'dan alıyoruz.
            basket.Items.Add(new BasketItem
            {
                ProductId = product.Id,
                ProductName = product.Name,
                Price = product.Price,
                Quantity = request.Quantity
            });
        }
        else
        {
            existing.Quantity += request.Quantity;
            existing.Price = product.Price; // güncel fiyatı yansıt
        }

        await _repository.UpdateBasketAsync(basket);
        return Result<BasketDto>.Success(basket.ToDto());
    }

    public async Task<Result<BasketDto>> RemoveItemAsync(string buyerId, int productId)
    {
        var basket = await _repository.GetBasketAsync(buyerId);
        if (basket is null)
            return Result<BasketDto>.Failure("Sepet bulunamadı.");

        var item = basket.Items.FirstOrDefault(i => i.ProductId == productId);
        if (item is not null)
        {
            basket.Items.Remove(item);
            await _repository.UpdateBasketAsync(basket);
        }

        return Result<BasketDto>.Success(basket.ToDto());
    }

    public async Task<Result> ClearAsync(string buyerId)
    {
        await _repository.DeleteBasketAsync(buyerId);
        return Result.Success();
    }
}
