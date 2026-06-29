using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Ordering.Infrastructure.Persistence;

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
        var context = scope.ServiceProvider.GetRequiredService<OrderingDbContext>();
        _logger.LogInformation("[SEED] Ordering veritabanı migrate ediliyor...");
        await context.Database.MigrateAsync(cancellationToken);
        _logger.LogInformation("[SEED] Migrate tamam.");
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
