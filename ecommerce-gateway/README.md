# API Gateway — E-Ticaret Mikroservis Platformu

Tüm servislerin önünde duran **tek giriş noktası** (reverse proxy). İstemci beş ayrı
servisi değil, sadece bu gateway'i (`http://localhost:8080`) bilir. Gateway, gelen
isteğin yoluna bakıp doğru servise iletir.

**YARP** (Yet Another Reverse Proxy — Microsoft'un .NET reverse proxy kütüphanesi) kullanır.

## Bu ilk sürümde kapsam

- **Sadece yönlendirme** (routing). Kimlik doğrulama şimdilik her serviste kendi
  içinde yapılıyor; gateway yalnızca isteği doğru servise iletir.
- Sonraki sürümde token doğrulama gateway'e taşınabilir (geçersiz token servise hiç ulaşmaz).

## Yönlendirme tablosu

| İstemci (gateway) | İletilen servis |
|---|---|
| `localhost:8080/identity/connect/token` | `localhost:5001/connect/token` |
| `localhost:8080/catalog/api/products`   | `localhost:5002/api/products` |
| `localhost:8080/basket/api/basket`      | `localhost:5003/api/basket` |
| `localhost:8080/orders/api/orders`      | `localhost:5004/api/orders` |
| `localhost:8080/payments/api/payments`  | `localhost:5005/api/payments` |

Gateway, önek'i (`/catalog` vb.) `PathRemovePrefix` ile kırpar; yani servise asıl yol
(`/api/products`) ulaşır. Route/cluster tanımları `appsettings.json` içindedir — kod değil,
yapılandırma.

## Nasıl çalışır (iki kavram)

- **Route:** "Hangi istek?" — yolu `/catalog/{**catch-all}` olanlar.
- **Cluster:** "Nereye gitsin?" — hedef `http://localhost:5002/`.

Her route bir cluster'a bağlıdır. Yeni bir servis eklemek = appsettings'e bir route + bir cluster eklemek.

## Çalıştırma

Arkadaki servisler açık olmalı (gateway sadece yönlendirir, kendisi iş yapmaz).

```bash
dotnet run --project src/Gateway.Api
```

Gateway: `http://localhost:8080`
Sağlık kontrolü: tarayıcıda `http://localhost:8080/` → kısa bir bilgi metni döner.

## Deneme

```bash
# Önce Identity açıkken token al (gateway üzerinden):
POST http://localhost:8080/identity/connect/token
Body (x-www-form-urlencoded): grant_type=password&username=admin&password=Admin123!&scope=ecommerce-api

# Sonra token'la Catalog'a (gateway üzerinden):
GET http://localhost:8080/catalog/api/products
Authorization: Bearer <token>
```

İstemci artık tek adres (`8080`) biliyor; portları (5001-5005) bilmesine gerek yok.

## Sonraki adım
- Gateway'de kimlik doğrulama (OpenIddict Validation) — geçersiz token'ı serviste değil burada elemek.
- Rate limiting, merkezî loglama, CORS.
- Tek docker-compose ile tüm servisler + gateway + altyapıyı birlikte ayağa kaldırma.
