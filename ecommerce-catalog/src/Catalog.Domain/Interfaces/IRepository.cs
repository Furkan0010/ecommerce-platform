using Catalog.Domain.Common;

namespace Catalog.Domain.Interfaces;

/// <summary>
/// Genel amaçlı repository. BlogPlatform'daki kalıbın aynısı.
/// </summary>
public interface IRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(int id);
    Task<IReadOnlyList<T>> GetAllAsync();
    Task AddAsync(T entity);
    void Update(T entity);
    void Remove(T entity);
    IQueryable<T> Query();
}
