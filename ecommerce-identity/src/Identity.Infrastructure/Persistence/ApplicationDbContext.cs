using Identity.Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Identity.Infrastructure.Persistence;

/// <summary>
/// Hem ASP.NET Identity tablolarını (kullanıcılar, roller...) hem de
/// OpenIddict tablolarını (uygulamalar, yetkiler, token'lar...) barındırır.
/// OpenIddict tabloları, DI tarafında çağrılan options.UseOpenIddict() ile
/// modele eklenir; burada ayrıca bir şey yapmaya gerek yoktur.
/// </summary>
public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }
}
