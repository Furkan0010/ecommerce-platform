# Basket Service — E-Ticaret Mikroservis Platformu

Kullanıcı sepeti servisi. Veriyi **Redis**'te tutar ve sepete ürün eklerken
**Catalog servisine senkron HTTP çağrısı** yaparak ürünü/fiyatı doğrular.

## Mimari

```
src/
  Basket.Domain          → CustomerBasket, BasketItem (ilişkisel değil; Redis için sade)
  Basket.Application     → DTO, Result<T>, IBasketService/BasketService, ICatalogClient, IBasketRepository, validatör
  Basket.Infrastructure  → RedisBasketRepository (StackExchange.Redis), CatalogClient (typed HttpClient + Polly)
  Basket.Api            → BasketController ([Authorize]), OpenIddict validation
tests/
  Basket.Tests          → BasketService birim testleri (sahte repo + sahte Catalog, altyapı gerektirmez)
```

## Bu serviste yeni olan iki kavram

**1. Redis ile depolama (EF yok).** Sepet kalıcı/ilişkisel veri değildir; hız
ister. `basket:{buyerId}` anahtarında JSON olarak saklanır. Migration yoktur.

**2. Servisler arası senkron çağrı.** Sepete ürün eklenirken Basket, Catalog'un
`GET /api/products/{id}` ucuna gider; ürün yoksa ekleme reddedilir, varsa **fiyat
ve ad Catalog'dan alınır** (istemciye güvenilmez). Bu çağrı Polly ile sarılır:

```csharp
services.AddHttpClient<ICatalogClient, CatalogClient>(c =>
        c.BaseAddress = new Uri(configuration["Catalog:BaseUrl"]!))
    .AddStandardResilienceHandler(); // retry + circuit breaker + timeout
```

Sepet kime ait? Token'daki `sub` (kullanıcı id) claim'i — yani kimlik doğrulama
doğrudan iş mantığına akar. Tüm uçlar `[Authorize]` (giriş zorunlu).

## Çalıştırma (yerel)

Bu servisi denemek için **Identity (5001), Catalog (5002) ve Redis** çalışıyor olmalı.

```bash
# 1) Redis'i ayağa kaldır
docker compose up -d basket-redis

# 2) Basket'i çalıştır (migration YOK — Redis kullanıyoruz)
dotnet run --project src/Basket.Api
```

Swagger: `http://localhost:5003/swagger`

## Deneme

```bash
# 1) Identity'den token al
TOKEN=$(curl -s -X POST http://localhost:5001/connect/token \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "grant_type=password&username=admin&password=Admin123!&scope=ecommerce-api" \
  | jq -r .access_token)

# 2) Sepete ürün ekle (Catalog'da seed edilen 1 numaralı ürünü dene)
curl -X POST http://localhost:5003/api/basket/items \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"productId":1,"quantity":2}'

# 3) Sepeti gör
curl http://localhost:5003/api/basket -H "Authorization: Bearer $TOKEN"
```

Token olmadan istek → **401**. Catalog kapalıyken ürün eklemeye çalışırsan,
Polly birkaç kez yeniden dener, sonra hata döner (servis bağımlılığı).

## Test

```bash
dotnet test
```

Testler Redis/Catalog gerektirmez; sahte (fake) implementasyonlarla çalışır.
Öne çıkan test: istemci farklı fiyat gönderse bile sepete **Catalog'un fiyatının**
yazıldığını doğrular.

## Notlar
- `Redis:Connection`, `Catalog:BaseUrl`, `Identity:Issuer` → `appsettings.json`.
- Redis verisi kalıcı değildir; container silinince sepetler gider (beklenen davranış).
