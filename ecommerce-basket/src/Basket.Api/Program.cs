using Basket.Api.Middleware;
using Basket.Application;
using Basket.Infrastructure;
using OpenIddict.Validation.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddInfrastructure(builder.Configuration); // Redis + Catalog client
builder.Services.AddApplication();                          // BasketService + validatörler

// Identity'nin ürettiği token'ı doğrula (Catalog ile aynı kurulum).

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
});

builder.Services.AddOpenIddict()
    .AddValidation(options =>
    {
        options.SetIssuer(builder.Configuration["Identity:Issuer"]!);
        options.AddAudiences("ecommerce-api");
        options.UseSystemNetHttp();
        options.UseAspNetCore();
    });

builder.Services.AddAuthorization();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ExceptionMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();

public partial class Program;
