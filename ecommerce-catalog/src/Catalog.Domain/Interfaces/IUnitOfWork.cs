using Catalog.Domain.Common;

namespace Catalog.Domain.Interfaces;

/// <summary>
/// Tüm repository'lere tek noktadan erişim ve tek bir SaveChanges.
/// BlogPlatform'daki UnitOfWork mantığının aynısı.
/// </summary>
public interface IUnitOfWork
{
    IRepository<T> Repository<T>() where T : BaseEntity;
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
