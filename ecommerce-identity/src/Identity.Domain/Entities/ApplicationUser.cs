using Microsoft.AspNetCore.Identity;

namespace Identity.Domain.Entities;

/// <summary>
/// Uygulamanın kullanıcı varlığı. ASP.NET Identity'nin IdentityUser'ını
/// genişletir; böylece parola yönetimi, kilitlenme, e-posta onayı gibi
/// yetenekleri hazır alırız. Domain'e özgü ek alanları buraya ekliyoruz.
/// </summary>
public class ApplicationUser : IdentityUser
{
    public string? FullName { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
