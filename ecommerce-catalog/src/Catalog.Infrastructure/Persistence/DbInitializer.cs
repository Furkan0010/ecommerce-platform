using Catalog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Catalog.Infrastructure.Persistence;

/// <summary>
/// Açılışta veritabanını migrate eder ve örnek kategori/ürün ekler ki
/// GET uçları ilk denemede dolu dönsün.
/// </summary>
public class DbInitializer : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DbInitializer> _logger;

    public DbInitializer(IServiceProvider serviceProvider, ILogger<DbInitializer> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();

        await context.Database.MigrateAsync(cancellationToken);

        if (!await context.Categories.AnyAsync(cancellationToken))
        {
            var electronics = new Category { Name = "Elektronik", Description = "Telefon, bilgisayar, aksesuar" };
            var books = new Category { Name = "Kitap", Description = "Roman, teknik, çocuk" };

            electronics.Products.Add(new Product { Name = "Kablosuz Kulaklık", Price = 1299.90m, StockQuantity = 50, Description = "Gürültü engelleyici" });
            electronics.Products.Add(new Product { Name = "Mekanik Klavye", Price = 899.00m, StockQuantity = 30 });
            books.Products.Add(new Product { Name = "Temiz Mimari", Price = 245.00m, StockQuantity = 100 });

            context.Categories.AddRange(electronics, books);
            await context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Örnek katalog verisi eklendi.");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
