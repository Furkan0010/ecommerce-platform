# Identity Service — E-Ticaret Mikroservis Platformu

Kimlik doğrulama ve yetkilendirme servisi. **OpenIddict** (OAuth2 / OIDC) ile
JWT access + refresh token üretir. Diğer tüm servisler bu servisin ürettiği
token'ları doğrular.

## Mimari

BlogPlatform ile aynı Clean Architecture düzeni:

```
src/
  Identity.Domain          → ApplicationUser (IdentityUser'ı genişletir)
  Identity.Application     → DTO, Result<T>, FluentValidation validatörleri
  Identity.Infrastructure  → ApplicationDbContext, Identity + OpenIddict + EF Core (SQL Server)
  Identity.Api             → Program.cs, token uç noktası, kayıt uç noktası
tests/
  Identity.Tests           → xUnit + WebApplicationFactory entegrasyon testi
```

## Gereksinimler
- .NET 9 SDK
- Docker (SQL Server ve konteyner çalıştırma için)
- `dotnet-ef` aracı: `dotnet tool install --global dotnet-ef`

## İlk kurulum (yerel)

```bash
# 1) SQL Server.i ayağa kaldir (sadece veritabani)
docker compose up -d identity-db

# 2) İlk migration'ı oluştur (EF Core, OpenIddict + Identity tablolarını üretir)
dotnet ef migrations add InitialCreate \
  --project src/Identity.Infrastructure \
  --startup-project src/Identity.Api

# 3) Servisi çalıştır (açılışta migrate + seed otomatik yapılır)
dotnet run --project src/Identity.Api
```

Swagger: `https://localhost:<port>/swagger`

> Açılışta bir demo admin oluşturulur: **admin / Admin123!**

## Tamamını Docker ile çalıştırma

```bash
docker compose up -d --build
# identity-api → http://localhost:5001
```

## Deneme

**Üye ol:**
```bash
curl -X POST http://localhost:5001/connect/register \
  -H "Content-Type: application/json" \
  -d '{"userName":"furkan","email":"[email protected]","password":"Sifre123!","fullName":"Furkan"}'
```

**Token al (password grant):**
```bash
curl -X POST http://localhost:5001/connect/token \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "grant_type=password&username=furkan&password=Sifre123!&scope=ecommerce-api offline_access"
```

Dönen `access_token` bir imzalı JWT'dir. `refresh_token` ise süre dolunca
yenilemek içindir (`grant_type=refresh_token`).

## Diğer servisler bu token'ı nasıl doğrulayacak?

Catalog/Basket/Ordering gibi servisler şu kurulumu yapacak (Faz 2'de detaylı):

```csharp
builder.Services.AddOpenIddict()
    .AddValidation(options =>
    {
        options.SetIssuer("http://identity-api:8080/");
        options.UseSystemNetHttp();   // JWKS'i Identity'den çeker
        options.UseAspNetCore();
    });
```

Servis, Identity'nin açtığı discovery (`/.well-known/openid-configuration`) ve
JWKS uçlarından imza anahtarını alır; her istekte Identity'e gitmeden token'ı
yerelde doğrular. Access token şifrelenmediği için (`DisableAccessTokenEncryption`)
bu doğrulama standart bir imzalı JWT doğrulamasıdır.

## Notlar
- Paket sürümleri .NET 9 ile uyumludur; NuGet'te en güncel **6.x** OpenIddict
  sürümüne yükseltebilirsin.
- Üretimde geliştirme sertifikaları gerçek sertifikalarla değiştirilmelidir.
