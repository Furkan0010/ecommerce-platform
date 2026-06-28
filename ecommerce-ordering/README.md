# Ordering Service — E-Ticaret Mikroservis Platformu

Sipariş servisi. Bu **ilk adımda** sipariş oluşturur, SQL Server'a kaydeder ve
**`OrderSubmitted` olayını RabbitMQ'ya (MassTransit + Outbox ile) yayınlar.**
Stok/ödeme/telafi (saga) sonraki adımlarda eklenecek.

## Mimari

```
src/
  Shared.Contracts        → Servisler arası olay sözleşmeleri (OrderSubmitted). Namespace: ECommerce.Shared.Contracts
  Ordering.Domain         → Order, OrderItem, OrderStatus, IRepository, IUnitOfWork
  Ordering.Application    → DTO, Result<T>, IOrderService/OrderService (olay YAYINLAR), validatör
  Ordering.Infrastructure → OrderingDbContext (+Outbox tabloları), Repository, MassTransit/RabbitMQ kurulumu, demo consumer
  Ordering.Api           → OrdersController, OpenIddict validation
tests/
  Ordering.Tests          → OrderService birim testleri (sipariş oluşturunca olay yayınlanıyor mu)
```

## Bu adımda yeni olan kavramlar

**1. Asenkron olay (RabbitMQ + MassTransit).** Sipariş oluşunca senkron HTTP
çağrısı yapmıyoruz; `OrderSubmitted` olayını **yayınlıyoruz**. Kim dinlerse o tepki verir.

**2. Outbox pattern.** Olayı doğrudan kuyruğa atmıyoruz. `OrderService`:
```csharp
await _publishEndpoint.Publish(new OrderSubmitted(...)); // outbox tablosuna yazılır
await _unitOfWork.SaveChangesAsync();                    // sipariş + olay ATOMİK commit
```
Böylece "siparişi kaydettim ama olayı yollayamadan çöktüm" durumu olmaz. MassTransit,
commit'ten sonra olayı arka planda RabbitMQ'ya iletir.

**3. Shared.Contracts.** Olay sözleşmeleri burada. Şimdilik bu repo içinde bir proje;
**Faz 5'te** Payment de bu olayları dinleyince ayrı bir repoya + NuGet paketine taşınacak.
Namespace (`ECommerce.Shared.Contracts`) sabit kaldığı için taşıma mesajları bozmaz.

**4. Demo consumer.** `OrderSubmittedConsumer` şu an olayı sadece loglar — yayınla→kuyruk→tüket
döngüsünü görmen için. Faz 5'te bu mantık gerçek servislere (stok, ödeme) dağılacak.

## Çalıştırma (yerel)

Gerekli: **Identity (5001)** açık olmalı (token doğrulama için) ve **RabbitMQ** çalışmalı.
SQL tarafı LocalDB kullanıyor (Docker gerekmez); RabbitMQ için Docker gerekir.

```bash
# 1) RabbitMQ'yu ayağa kaldır
docker compose up -d ordering-rabbitmq

# 2) İlk migration (Order tabloları + Outbox tabloları)
dotnet ef migrations add InitialCreate --project src/Ordering.Infrastructure --startup-project src/Ordering.Api

# 3) Çalıştır
dotnet run --project src/Ordering.Api
```

- Swagger: `http://localhost:5004/swagger`
- RabbitMQ yönetim arayüzü: `http://localhost:15672` (guest / guest) — exchange/queue ve mesajları buradan görebilirsin.

## Deneme

```bash
# 1) Identity'den token al (admin ya da herhangi bir kullanıcı)
TOKEN=...

# 2) Sipariş oluştur
curl -X POST http://localhost:5004/api/orders \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"items":[{"productId":1,"productName":"Telefon","unitPrice":5000,"quantity":2}]}'
```

Beklenen: sipariş `Pending` kaydedilir, cevap döner. Konsolda demo consumer'ın logu:
```
[EVENT] OrderSubmitted alındı -> OrderId: ..., Tutar: 10000, ...
```
Bu log, olayın RabbitMQ üzerinden gidip geri tüketildiğini kanıtlar.

## Test

```bash
dotnet test
```
RabbitMQ/SQL gerektirmez; `OrderService`'in sipariş oluşturunca `OrderSubmitted`
yayınladığını sahte bağımlılıklarla doğrular.

## Sonraki adımlar (Faz 4 devamı / Faz 5)
- Saga state machine: Pending → StockReserved → Paid → Confirmed, ve telafi.
- Catalog'a stok rezervasyonu consumer'ı, Payment servisi.
- Shared.Contracts'ın ayrı repoya + NuGet'e taşınması.
- "Checkout" akışı: sipariş kalemlerini istemci yerine Basket servisinden çekmek.
