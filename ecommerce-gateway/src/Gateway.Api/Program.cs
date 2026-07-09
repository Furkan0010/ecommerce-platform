var builder = WebApplication.CreateBuilder(args);

// YARP'ı appsettings.json'daki "ReverseProxy" bölümünden yükle.
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();

// Basit bir sağlık ucu: gateway ayakta mı diye bakmak için.
app.MapGet("/", () => "E-Ticaret API Gateway calisiyor. Servisler: /identity /catalog /basket /orders /payments");

// Tüm eşleşen istekleri route/cluster tanımlarına göre arka servislere ilet.
app.MapReverseProxy();

app.Run();
