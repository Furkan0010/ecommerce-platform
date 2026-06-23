using Catalog.Application.Common;
using Catalog.Application.DTOs;

namespace Catalog.Application.Interfaces;

public interface ICategoryService
{
    Task<Result<IReadOnlyList<CategoryDto>>> GetAllAsync();
    Task<Result<CategoryDto>> CreateAsync(CreateCategoryRequest request);
}
