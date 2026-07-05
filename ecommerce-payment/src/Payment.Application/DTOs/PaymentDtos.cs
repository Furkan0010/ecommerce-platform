using Payment.Domain.Entities;

namespace Payment.Application.DTOs;

public record ProcessPaymentRequest(Guid OrderNumber, decimal Amount);

public record PaymentDto(
    int Id,
    Guid OrderNumber,
    string BuyerId,
    decimal Amount,
    PaymentStatus Status,
    string? ProviderReference,
    string? FailureReason);
