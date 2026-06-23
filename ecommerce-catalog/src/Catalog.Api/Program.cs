using Catalog.Api.Middleware;
using Catalog.Application;
using Catalog.Infrastructure;
using OpenIddict.Validation.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// --- Katmanların DI kaydı ---
builder.Services.AddInfrastructure(builder.Configuration); // DbContext + repository + UnitOfWork
builder.Services.AddApplication();                          // servisler + validatörler

// --- Kimlik doğrulama: Identity servisinin ürettiği token'ı DOĞRULA ---
// Catalog token üretmez; sadece gelen token'ı Identity'nin JWKS'iyle doğrular.

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
});

builder.Services.AddOpenIddict()
    .AddValidation(options =>
    {
        // Token'ı kimin ürettiği (Identity servisinin adresi). Tokendaki "iss" ile eşleşmeli.
        options.SetIssuer(builder.Configuration["Identity:Issuer"]!);

        // Bu API'nin beklediği audience; Identity tarafında SetResources("ecommerce-api") ile eşleşir.
        options.AddAudiences("ecommerce-api");

        // İmza anahtarını (JWKS) Identity'den HTTP ile çeker.
        options.UseSystemNetHttp();

        // ASP.NET Core entegrasyonu.
        options.UseAspNetCore();
    });

builder.Services.AddAuthorization(options =>
{
    // "AdminOnly": token içinde role=Admin claim'i olmalı.
    // RequireClaim("role", ...) kullanıyoruz; rol claim tipi eşlemesine bağımlı değil, garantili çalışır.
    options.AddPolicy("AdminOnly", policy => policy.RequireClaim("role", "Admin"));
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Merkezi hata yönetimi en başta.
app.UseMiddleware<ExceptionMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

// Entegrasyon testlerinin WebApplicationFactory ile erişebilmesi için.
public partial class Program;
