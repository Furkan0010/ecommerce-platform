using ECommerce.Shared.Contracts;
using FluentAssertions;
using MassTransit;
using NSubstitute;
using Ordering.Application.DTOs;
using Ordering.Application.Interfaces;
using Ordering.Application.Services;
using Ordering.Application.Validators;
using Xunit;

namespace Ordering.Tests;

public class OrderServiceTests
{
    [Fact]
    public async Task CreateOrder_Should_Save_And_Publish_OrderSubmitted()
    {
        var uow = new FakeUnitOfWork();
        var publish = Substitute.For<IPublishEndpoint>();
        var sut = new OrderService(uow, publish, new CreateOrderRequestValidator());

        var request = new CreateOrderRequest(new List<CreateOrderItem>
        {
            new(ProductId: 1, ProductName: "Telefon", UnitPrice: 5000m, Quantity: 2)
        });

        var result = await sut.CreateOrderAsync("user-1", request);

        result.Succeeded.Should().BeTrue();
        result.Data!.Total.Should().Be(10000m);

        // OrderSubmitted olayının yayınlandığını doğrula.
        await publish.Received(1).Publish(Arg.Any<OrderSubmitted>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateOrder_Should_Fail_When_No_Items()
    {
        var sut = new OrderService(new FakeUnitOfWork(), Substitute.For<IPublishEndpoint>(), new CreateOrderRequestValidator());

        var result = await sut.CreateOrderAsync("user-1", new CreateOrderRequest(new List<CreateOrderItem>()));

        result.Succeeded.Should().BeFalse();
    }
}
