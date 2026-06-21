using Identity.Application;
using Identity.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// --- Katmanların DI kaydı ---
builder.Services.AddInfrastructure(builder.Configuration); // DbContext + Identity + OpenIddict core
builder.Services.AddApplication();                          // FluentValidation validatörleri

// --- OpenIddict sunucu (token üretimi) ve doğrulama (bu servisin kendi uçları için) ---
builder.Services.AddOpenIddict()
    .AddServer(options =>
    {
        // Token uç noktası: /connect/token
        options.SetTokenEndpointUris("connect/token");

        // Bu sürümde desteklenen akışlar.
        options.AllowPasswordFlow()
               .AllowRefreshTokenFlow();

        // İstemci sırrı olmadan (public client / curl ile) token alınabilsin.
        options.AcceptAnonymousClients();

        // Servislerin kullanacağı özel scope'lar.
        options.RegisterScopes(
            "ecommerce-api",
            "catalog.read", "catalog.write",
            "basket",
            "order.read", "order.write");

        // Geliştirme sertifikaları (üretimde gerçek sertifikalarla değiştirilir).
        options.AddDevelopmentEncryptionCertificate()
               .AddDevelopmentSigningCertificate();

        // ÖNEMLİ: Access token'ı şifrelemiyoruz; böylece diğer servisler onu
        // standart bir imzalı JWT olarak, JWKS üzerinden kolayca doğrulayabilir.
        options.DisableAccessTokenEncryption();

        options.SetAccessTokenLifetime(TimeSpan.FromMinutes(15));
        options.SetRefreshTokenLifetime(TimeSpan.FromDays(7));

        // ASP.NET Core entegrasyonu: token uç noktasını controller'a aç.
        var aspNetCore = options.UseAspNetCore()
                                .EnableTokenEndpointPassthrough();

        // Yerelde (Development) http üzerinden test edebilmek için HTTPS zorunluluğunu
        // kaldırıyoruz. Üretimde bu satır çalışmaz, HTTPS zorunlu kalır.
        if (builder.Environment.IsDevelopment())
            aspNetCore.DisableTransportSecurityRequirement();
    })
    .AddValidation(options =>
    {
        // Bu servisin kendi korumalı uçları için yerel doğrulama.
        options.UseLocalServer();
        options.UseAspNetCore();
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

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

// Entegrasyon testlerinin WebApplicationFactory ile erişebilmesi için.
public partial class Program;
