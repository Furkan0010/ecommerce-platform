using Payment.Application.Interfaces;

namespace Payment.Infrastructure.Gateway;

/// <summary>
/// Sahte ödeme sağlayıcısı. Gerçek bir Stripe/iyzico yerine deterministik bir
/// kuralla çalışır: 50.000 üstü tutarları "limit aşıldı" diye reddeder, diğerlerini
/// onaylayıp sahte bir referans döner. Böylece hem başarı hem başarısızlık yolunu
/// (ileride saga'da telafi) test edebilirsin.
/// </summary>
public class FakePaymentGateway : IPaymentGateway
{
    private const decimal Limit = 50000m;

    public Task<PaymentGatewayResult> ChargeAsync(decimal amount, string buyerId)
    {
        if (amount > Limit)
            return Task.FromResult(new PaymentGatewayResult(false, null, "Tutar limiti aşıldı."));

        var reference = $"FAKE-{Guid.NewGuid():N}";
        return Task.FromResult(new PaymentGatewayResult(true, reference, null));
    }
}
