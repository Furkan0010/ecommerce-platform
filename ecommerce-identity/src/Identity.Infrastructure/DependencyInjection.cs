using Identity.Domain.Entities;
using Identity.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace Identity.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, IConfiguration configuration)
    {
        // 1) Veritabanı: SQL Server + OpenIddict tablolarını modele ekle.
        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseSqlServer(configuration.GetConnectionString("IdentityDb"));
            options.UseOpenIddict();
        });

        // 2) ASP.NET Identity: kullanıcı/rol yönetimi.
        services.AddIdentity<ApplicationUser, IdentityRole>(options =>
        {
            options.Password.RequiredLength = 6;
            options.Password.RequireNonAlphanumeric = false;
            options.User.RequireUniqueEmail = true;
            options.SignIn.RequireConfirmedAccount = false;

            // Identity'nin ürettiği claim tiplerini OpenIddict'in beklediği
            // standart adlara hizalıyoruz. Bu, token üretimini sorunsuzlaştırır.
            options.ClaimsIdentity.UserNameClaimType = Claims.Name;
            options.ClaimsIdentity.UserIdClaimType = Claims.Subject;
            options.ClaimsIdentity.RoleClaimType = Claims.Role;
        })
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();

        // 3) OpenIddict çekirdeği: EF Core üzerinden veri saklama.
        services.AddOpenIddict()
            .AddCore(options =>
            {
                options.UseEntityFrameworkCore()
                       .UseDbContext<ApplicationDbContext>();
            });

        // Veritabanı migrate + seed.
        services.AddHostedService<DbInitializer>();

        return services;
    }
}
