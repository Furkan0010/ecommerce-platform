using Basket.Application.DTOs;
using Basket.Application.Interfaces;
using Basket.Application.Services;
using Basket.Application.Validators;
using FluentAssertions;
using Xunit;

namespace Basket.Tests;

public class BasketServiceTests
{
    private static BasketService CreateSut(ProductInfo? product)
        => new(new FakeBasketRepository(), new FakeCatalogClient(product), new AddItemRequestValidator());

    [Fact]
    public async Task AddItem_Should_Use_Catalog_Price_Not_Client_Price()
    {
        var product = new ProductInfo(1, "Telefon", 5000m, 10);
        var sut = CreateSut(product);

        var result = await sut.AddItemAsync("user-1", new AddItemRequest(ProductId: 1, Quantity: 2));

        result.Succeeded.Should().BeTrue();
        result.Data!.Items.Should().ContainSingle();
        result.Data.Items[0].Price.Should().Be(5000m);   // Catalog'dan gelen fiyat
        result.Data.Total.Should().Be(10000m);            // 5000 * 2
    }

    [Fact]
    public async Task AddItem_Should_Fail_When_Product_Not_Found()
    {
        var sut = CreateSut(product: null); // Catalog ürünü bulamıyor

        var result = await sut.AddItemAsync("user-1", new AddItemRequest(ProductId: 99, Quantity: 1));

        result.Succeeded.Should().BeFalse();
    }

    [Fact]
    public async Task AddItem_Should_Fail_When_Stock_Insufficient()
    {
        var product = new ProductInfo(1, "Telefon", 5000m, 1); // stok 1
        var sut = CreateSut(product);

        var result = await sut.AddItemAsync("user-1", new AddItemRequest(ProductId: 1, Quantity: 5));

        result.Succeeded.Should().BeFalse();
    }
}
