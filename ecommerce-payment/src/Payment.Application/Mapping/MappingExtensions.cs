using Payment.Application.DTOs;
using PaymentEntity = Payment.Domain.Entities.Payment;

namespace Payment.Application.Mapping;

public static class MappingExtensions
{
    public static PaymentDto ToDto(this PaymentEntity p) => new(
        p.Id, p.OrderNumber, p.BuyerId, p.Amount, p.Status, p.ProviderReference, p.FailureReason);
}
