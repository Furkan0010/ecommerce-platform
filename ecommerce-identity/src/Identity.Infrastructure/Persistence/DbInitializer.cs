using Identity.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Identity.Infrastructure.Persistence;

/// <summary>
/// Açılışta veritabanını migrate eder, rolleri ve demo admin'i oluşturur.
/// Bu sürüm her adımı loglar ve hataları YUTMAZ; böylece bir şey ters giderse
/// sebebini konsolda görürüz.
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
        var sp = scope.ServiceProvider;

        var context = sp.GetRequiredService<ApplicationDbContext>();

        _logger.LogInformation("[SEED] Migrate başlıyor...");
        await context.Database.MigrateAsync(cancellationToken);
        _logger.LogInformation("[SEED] Migrate tamam.");

        // --- Roller ---
        var roleManager = sp.GetRequiredService<RoleManager<IdentityRole>>();
        foreach (var role in new[] { "Admin", "Customer" })
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                var r = await roleManager.CreateAsync(new IdentityRole(role));
                if (r.Succeeded)
                    _logger.LogInformation("[SEED] Rol oluşturuldu: {Role}", role);
                else
                    _logger.LogError("[SEED] Rol OLUŞTURULAMADI: {Role} -> {Errors}",
                        role, string.Join(" | ", r.Errors.Select(e => e.Description)));
            }
        }

        // --- Demo admin ---
        var userManager = sp.GetRequiredService<UserManager<ApplicationUser>>();
        var existing = await userManager.FindByNameAsync("admin");
        if (existing is not null)
        {
            _logger.LogWarning("[SEED] 'admin' kullanıcısı ZATEN VAR, yeniden oluşturulmadı.");
            return;
        }

        var admin = new ApplicationUser
        {
            UserName = "admin",
            Email = "admin@example.com",
            EmailConfirmed = true,
            FullName = "Sistem Yöneticisi"
        };

        var result = await userManager.CreateAsync(admin, "Admin123!");
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(admin, "Admin");
            _logger.LogInformation("[SEED] >>> Demo admin oluşturuldu (admin / Admin123!).");
        }
        else
        {
            _logger.LogError("[SEED] >>> ADMIN OLUŞTURULAMADI: {Errors}",
                string.Join(" | ", result.Errors.Select(e => e.Description)));
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
