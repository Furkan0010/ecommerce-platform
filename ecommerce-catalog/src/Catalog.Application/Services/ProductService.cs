using Catalog.Application.Common;
using Catalog.Application.DTOs;
using Catalog.Application.Interfaces;
using Catalog.Application.Mapping;
using Catalog.Domain.Entities;
using Catalog.Domain.Interfaces;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Application.Services;

/// <summary>
/// Ürün iş mantığı. BlogPlatform'daki PostService ile aynı tarz:
/// klasik DI (private readonly + kurucu), Result&lt;T&gt; dönüşleri,
/// repository + UnitOfWork üzerinden veri erişimi.
/// </summary>
public class ProductService : IProductService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<CreateProductRequest> _createValidator;
    private readonly IValidator<UpdateProductRequest> _updateValidator;

    public ProductService(
        IUnitOfWork unitOfWork,
        IValidator<CreateProductRequest> createValidator,
        IValidator<UpdateProductRequest> updateValidator)
    {
        _unitOfWork = unitOfWork;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    public async Task<Result<IReadOnlyList<ProductDto>>> GetAllAsync()
    {
        var products = await _unitOfWork.Repository<Product>()
            .Query()
            .Include(p => p.Category)
            .ToListAsync();

        var dtos = products.Select(p => p.ToDto()).ToList();
        return Result<IReadOnlyList<ProductDto>>.Success(dtos);
    }

    public async Task<Result<ProductDto>> GetByIdAsync(int id)
    {
        var product = await _unitOfWork.Repository<Product>()
            .Query()
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product is null)
            return Result<ProductDto>.Failure($"Ürün bulunamadı (Id: {id}).");

        return Result<ProductDto>.Success(product.ToDto());
    }

    public async Task<Result<ProductDto>> CreateAsync(CreateProductRequest request)
    {
        var validation = await _createValidator.ValidateAsync(request);
        if (!validation.IsValid)
            return Result<ProductDto>.Failure(validation.Errors.Select(e => e.ErrorMessage));

        // İlişkili kategori gerçekten var mı?
        var category = await _unitOfWork.Repository<Category>().GetByIdAsync(request.CategoryId);
        if (category is null)
            return Result<ProductDto>.Failure($"Kategori bulunamadı (Id: {request.CategoryId}).");

        var product = new Product
        {
            Name = request.Name,
            Description = request.Description,
            Price = request.Price,
            StockQuantity = request.StockQuantity,
            CategoryId = request.CategoryId
        };

        await _unitOfWork.Repository<Product>().AddAsync(product);
        await _unitOfWork.SaveChangesAsync();

        product.Category = category;
        return Result<ProductDto>.Success(product.ToDto());
    }

    public async Task<Result> UpdateAsync(int id, UpdateProductRequest request)
    {
        var validation = await _updateValidator.ValidateAsync(request);
        if (!validation.IsValid)
            return Result.Failure(validation.Errors.Select(e => e.ErrorMessage));

        var repo = _unitOfWork.Repository<Product>();
        var product = await repo.GetByIdAsync(id);
        if (product is null)
            return Result.Failure($"Ürün bulunamadı (Id: {id}).");

        var category = await _unitOfWork.Repository<Category>().GetByIdAsync(request.CategoryId);
        if (category is null)
            return Result.Failure($"Kategori bulunamadı (Id: {request.CategoryId}).");

        product.Name = request.Name;
        product.Description = request.Description;
        product.Price = request.Price;
        product.StockQuantity = request.StockQuantity;
        product.CategoryId = request.CategoryId;
        product.UpdatedAt = DateTime.UtcNow;

        repo.Update(product);
        await _unitOfWork.SaveChangesAsync();
        return Result.Success();
    }

    public async Task<Result> DeleteAsync(int id)
    {
        var repo = _unitOfWork.Repository<Product>();
        var product = await repo.GetByIdAsync(id);
        if (product is null)
            return Result.Failure($"Ürün bulunamadı (Id: {id}).");

        repo.Remove(product);
        await _unitOfWork.SaveChangesAsync();
        return Result.Success();
    }
}
