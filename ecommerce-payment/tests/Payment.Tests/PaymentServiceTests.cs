using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Payment.Application.DTOs;
using Payment.Application.Services;
using Payment.Application.Validators;
using Payment.Domain.Entities;
using Payment.Infrastructure.Gateway;
using Payment.Infrastructure.Persistence;
using Payment.Infrastructure.Repositories;
using Xunit;

namespace Payment.Tests;

public class PaymentServiceTests
{
    private static PaymentService CreateSut()
    {
        var options = new DbContextOptionsBuilder<PaymentDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var context = new PaymentDbContext(options);
        var uow = new UnitOfWork(context);
        return new PaymentService(uow, new FakePaymentGateway(), new ProcessPaymentRequestValidator());
    }

    [Fact]
    public async Task Process_Should_Succeed_When_Amount_Within_Limit()
    {
        var sut = CreateSut();
        var result = await sut.ProcessAsync("user-1", new ProcessPaymentRequest(Guid.NewGuid(), 10000m));

        result.Succeeded.Should().BeTrue();
        result.Data!.Status.Should().Be(PaymentStatus.Succeeded);
        result.Data.ProviderReference.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Process_Should_Fail_When_Amount_Exceeds_Limit()
    {
        var sut = CreateSut();
        var result = await sut.ProcessAsync("user-1", new ProcessPaymentRequest(Guid.NewGuid(), 99999m));

        // Kayıt başarılı ama ödeme durumu Failed olmalı (limit aşıldı).
        result.Succeeded.Should().BeTrue();
        result.Data!.Status.Should().Be(PaymentStatus.Failed);
        result.Data.FailureReason.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Process_Should_Reject_Invalid_Amount()
    {
        var sut = CreateSut();
        var result = await sut.ProcessAsync("user-1", new ProcessPaymentRequest(Guid.NewGuid(), 0m));

        result.Succeeded.Should().BeFalse(); // validasyon hatası
    }
}
