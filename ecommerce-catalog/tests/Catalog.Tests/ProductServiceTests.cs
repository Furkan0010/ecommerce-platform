using Catalog.Application.DTOs;
using Catalog.Application.Services;
using Catalog.Application.Validators;
using Catalog.Domain.Entities;
using Catalog.Infrastructure.Persistence;
using Catalog.Infrastructure.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Catalog.Tests;

/// <summary>
/// ProductService birim testleri. EF Core InMemory sağlayıcısıyla gerçek
/// repository + UnitOfWork üzerinden çalışır; veritabanı gerektirmez.
/// </summary>
public class ProductServiceTests
{
    private static (ProductService service, CatalogDbContext context) CreateSut()
    {
        var options = new DbContextOptionsBuilder<CatalogDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var context = new CatalogDbContext(options);
        var uow = new UnitOfWork(context);
        var service = new ProductService(
            uow,
            new CreateProductRequestValidator(),
            new UpdateProductRequestValidator());

        return (service, context);
    }

    [Fact]
    public async Task CreateAsync_Should_Succeed_When_Category_Exists()
    {
        var (service, context) = CreateSut();
        context.Categories.Add(new Category { Id = 1, Name = "Elektronik" });
        await context.SaveChangesAsync();

        var request = new CreateProductRequest("Telefon", "Açıklama", 9999.90m, 10, 1);

        var result = await service.CreateAsync(request);

        result.Succeeded.Should().BeTrue();
        result.Data!.Name.Should().Be("Telefon");
        context.Products.Should().ContainSingle();
    }

    [Fact]
    public async Task CreateAsync_Should_Fail_When_Category_Missing()
    {
        var (service, _) = CreateSut();

        var request = new CreateProductRequest("Telefon", null, 9999.90m, 10, 999);

        var result = await service.CreateAsync(request);

        result.Succeeded.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
    }

    [Fact]
    public async Task CreateAsync_Should_Fail_When_Price_Invalid()
    {
        var (service, context) = CreateSut();
        context.Categories.Add(new Category { Id = 1, Name = "Elektronik" });
        await context.SaveChangesAsync();

        var request = new CreateProductRequest("Telefon", null, 0m, 10, 1);

        var result = await service.CreateAsync(request);

        result.Succeeded.Should().BeFalse();
    }
}
