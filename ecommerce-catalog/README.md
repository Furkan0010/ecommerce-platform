# Catalog Service — E-Ticaret Mikroservis Platformu

Ürün ve kategori kataloğu servisi. Kendi iş mantığını (servis + repository +
UnitOfWork + validatör) içerir ve **Identity servisinin ürettiği token'ları
doğrulayarak** yazma uçlarını korur.

## Mimari (BlogPlatform ile aynı düzen)

```
src/
  Catalog.Domain          → BaseEntity, Product, Category, IRepository, IUnitOfWork
  Catalog.Application     → DTO, Result<T>, IProductService/ProductService, validatörler, manuel mapping
  Catalog.Infrastructure  → CatalogDbContext, Repository<T>, UnitOfWork, EF Core (SQL Server), seed
  Catalog.Api            → ProductsController, CategoriesController, ExceptionMiddleware, OpenIddict validation
tests/
  Catalog.Tests          → ProductService birim testleri (EF Core InMemory)
```

## Önemli: kimlik doğrulama bağlantısı

Catalog **token üretmez**, sadece doğrular. `Program.cs` içinde:

```csharp
builder.Services.AddOpenIddict().AddValidation(options =>
{
    options.SetIssuer(builder.Configuration["Identity:Issuer"]!); // Identity adresi
    options.AddAudiences("ecommerce-api");
    options.UseSystemNetHttp();   // imza anahtarını Identity'nin JWKS ucundan çeker
    options.UseAspNetCore();
});
```

- Okuma uçları (`GET`) herkese açık.
- Yazma uçları (`POST/PUT/DELETE`) `AdminOnly` politikasıyla korunur → token içinde `role=Admin` olmalı.

> Yazma uçlarını test ederken **Identity servisi de çalışıyor olmalı**, çünkü
> Catalog imza anahtarını ondan çeker. Token'ı Identity'den `admin / Admin123!`
> ile alıp kullanabilirsin.

## Çalıştırma (yerel)

```bash
# 1) Veritabanını ayağa kaldır (sadece SQL Server)
docker compose up -d catalog-db

# 2) İlk migration
dotnet ef migrations add InitialCreate --project src/Catalog.Infrastructure --startup-project src/Catalog.Api

# 3) Çalıştır (açılışta migrate + örnek veri seed edilir)
dotnet run --project src/Catalog.Api
```

Swagger: `http://localhost:5002/swagger`

## Deneme

**Ürünleri listele (token gerekmez):**
```bash
curl http://localhost:5002/api/products
```

**Ürün ekle (Admin token gerekir):**
```bash
# 1) Identity'den token al
TOKEN=$(curl -s -X POST http://localhost:5001/connect/token \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "grant_type=password&username=admin&password=Admin123!&scope=ecommerce-api" \
  | jq -r .access_token)

# 2) Token ile ürün oluştur
curl -X POST http://localhost:5002/api/products \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"name":"Yeni Ürün","description":"...","price":199.90,"stockQuantity":25,"categoryId":1}'
```

Token olmadan `POST` denersen **401**, Admin olmayan bir token ile denersen **403** alırsın.

## Test

```bash
dotnet test
```

(ProductService testleri InMemory ile çalışır, veritabanı gerektirmez.)

## Notlar
- Veritabanı: kendi `CatalogDb`'si (database-per-service ilkesi).
- Mapping AutoMapper yerine manuel (lisans nedeniyle) — `MappingExtensions`.
- Identity adresi `appsettings.json` → `Identity:Issuer` ile yönetilir; token'daki
  `iss` ile birebir eşleşmelidir (yerelde `http://localhost:5001/`).
