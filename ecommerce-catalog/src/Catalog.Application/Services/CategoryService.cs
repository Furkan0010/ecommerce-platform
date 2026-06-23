using Catalog.Application.Common;
using Catalog.Application.DTOs;
using Catalog.Application.Interfaces;
using Catalog.Application.Mapping;
using Catalog.Domain.Entities;
using Catalog.Domain.Interfaces;
using FluentValidation;

namespace Catalog.Application.Services;

public class CategoryService : ICategoryService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<CreateCategoryRequest> _createValidator;

    public CategoryService(IUnitOfWork unitOfWork, IValidator<CreateCategoryRequest> createValidator)
    {
        _unitOfWork = unitOfWork;
        _createValidator = createValidator;
    }

    public async Task<Result<IReadOnlyList<CategoryDto>>> GetAllAsync()
    {
        var categories = await _unitOfWork.Repository<Category>().GetAllAsync();
        var dtos = categories.Select(c => c.ToDto()).ToList();
        return Result<IReadOnlyList<CategoryDto>>.Success(dtos);
    }

    public async Task<Result<CategoryDto>> CreateAsync(CreateCategoryRequest request)
    {
        var validation = await _createValidator.ValidateAsync(request);
        if (!validation.IsValid)
            return Result<CategoryDto>.Failure(validation.Errors.Select(e => e.ErrorMessage));

        var category = new Category
        {
            Name = request.Name,
            Description = request.Description
        };

        await _unitOfWork.Repository<Category>().AddAsync(category);
        await _unitOfWork.SaveChangesAsync();

        return Result<CategoryDto>.Success(category.ToDto());
    }
}
