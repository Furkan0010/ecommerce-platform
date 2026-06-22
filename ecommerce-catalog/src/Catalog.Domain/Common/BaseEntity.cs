namespace Catalog.Domain.Common;

/// <summary>
/// BlogPlatform'daki BaseEntity ile aynı: tüm varlıkların ortak alanları.
/// </summary>
public abstract class BaseEntity
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
