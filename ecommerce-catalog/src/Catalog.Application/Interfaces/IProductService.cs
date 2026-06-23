using Catalog.Application.Common;
using Catalog.Application.DTOs;

namespace Catalog.Application.Interfaces;

public interface IProductService
{
    Task<Result<IReadOnlyList<ProductDto>>> GetAllAsync();
    Task<Result<ProductDto>> GetByIdAsync(int id);
    Task<Result<ProductDto>> CreateAsync(CreateProductRequest request);
    Task<Result> UpdateAsync(int id, UpdateProductRequest request);
    Task<Result> DeleteAsync(int id);
}
