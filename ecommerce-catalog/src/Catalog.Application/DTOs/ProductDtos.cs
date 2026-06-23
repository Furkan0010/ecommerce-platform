namespace Catalog.Application.DTOs;

public record ProductDto(
    int Id,
    string Name,
    string? Description,
    decimal Price,
    int StockQuantity,
    int CategoryId,
    string? CategoryName);

public record CreateProductRequest(
    string Name,
    string? Description,
    decimal Price,
    int StockQuantity,
    int CategoryId);

public record UpdateProductRequest(
    string Name,
    string? Description,
    decimal Price,
    int StockQuantity,
    int CategoryId);
