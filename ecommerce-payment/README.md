# Payment Service — E-Ticaret Mikroservis Platformu

Ödeme servisi. Ödemeleri işler ve kayıt altına alır. Gerçek bir ödeme sağlayıcısı
yerine şimdilik **sahte bir gateway** (`IPaymentGateway` → `FakePaymentGateway`) kullanır.

## Mimari

```
src/
  Payment.Domain          → Payment, PaymentStatus, IRepository, IUnitOfWork
  Payment.Application     → DTO, Result<T>, IPaymentService/PaymentService, IPaymentGateway (soyutlama), validatör
  Payment.Infrastructure  → PaymentDbContext, Repository, UnitOfWork, FakePaymentGateway, EF Core (SQL Server)
  Payment.Api            → PaymentsController, OpenIddict validation
tests/
  Payment.Tests           → PaymentService birim testleri (EF InMemory + sahte gateway)
```

## Bu serviste yeni olan kavram: dış servis soyutlaması

Gerçek ödeme sağlayıcısı (Stripe, iyzico...) bir arayüzün arkasında:

```csharp
public interface IPaymentGateway
{
    Task<PaymentGatewayResult> ChargeAsync(decimal amount, string buyerId);
}
```

Şu an `FakePaymentGateway` bunu dolduruyor — deterministik bir kural: **50.000 üstü
tutarlar reddedilir** ("limit aşıldı"), diğerleri onaylanıp sahte referans döner.
Gerçek sağlayıcıya geçince sadece bu implementasyon + DI satırı değişir, üst katmanlar
hiç değişmez. (Başarı/başarısızlık ayrımı, ileride saga'daki telafiyi test etmek için.)

## Çalıştırma (yerel)

Gerekli: **Identity (5001)** açık olmalı (token doğrulama). SQL tarafı LocalDB kullanır.

```bash
# (LocalDB kullanıyorsan Docker gerekmez. Konteyner DB istersen: docker compose up -d payment-db)

# İlk migration
dotnet ef migrations add InitialCreate --project src/Payment.Infrastructure --startup-project src/Payment.Api

# Çalıştır
dotnet run --project src/Payment.Api
```

Swagger: `http://localhost:5005/swagger`

## Deneme

```bash
# 1) Identity'den token al, sonra:

# Başarılı ödeme (limit altı)
curl -X POST http://localhost:5005/api/payments \
  -H "Authorization: Bearer $TOKEN" -H "Content-Type: application/json" \
  -d '{"orderNumber":"11111111-1111-1111-1111-111111111111","amount":10000}'
# -> status: 1 (Succeeded), providerReference dolu

# Başarısız ödeme (limit üstü)
curl -X POST http://localhost:5005/api/payments \
  -H "Authorization: Bearer $TOKEN" -H "Content-Type: application/json" \
  -d '{"orderNumber":"22222222-2222-2222-2222-222222222222","amount":99999}'
# -> status: 2 (Failed), failureReason: "Tutar limiti aşıldı."
```

`orderNumber` ile ödeme durumunu sorgula: `GET /api/payments/{orderNumber}`.

## Test

```bash
dotnet test
```
SQL/sağlayıcı gerektirmez (InMemory + sahte gateway). Hem onay hem ret yolunu test eder.

## Sonraki adım: Ordering saga'sına bağlama
Şu an Payment **tek başına HTTP'den** çalışıyor. Bir sonraki adımda:
- `Shared.Contracts` ayrı repoya + NuGet'e taşınacak (Ordering ile ortak kullanım için).
- Payment, RabbitMQ üzerinden `ProcessPayment` komutunu dinleyip `PaymentSucceeded`/`PaymentFailed`
  olaylarını yayınlayacak.
- Ordering'in saga'sı: OrderSubmitted → ödeme iste → başarılıysa Confirmed, başarısızsa Cancelled (telafi).
