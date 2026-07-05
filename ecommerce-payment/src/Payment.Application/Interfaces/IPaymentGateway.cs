namespace Payment.Application.Interfaces;

/// <summary>
/// Ödeme sağlayıcısı soyutlaması. Şu an sahte (fake) implementasyonu var;
/// ileride Stripe/iyzico gibi gerçek bir sağlayıcı bu arayüzü doldurur,
/// üst katmanlar hiç değişmez.
/// </summary>
public interface IPaymentGateway
{
    Task<PaymentGatewayResult> ChargeAsync(decimal amount, string buyerId);
}

public record PaymentGatewayResult(bool Approved, string? Reference, string? DeclineReason);
