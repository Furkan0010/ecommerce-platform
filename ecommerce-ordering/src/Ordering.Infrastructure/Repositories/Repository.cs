using Microsoft.EntityFrameworkCore;
using Ordering.Domain.Common;
using Ordering.Domain.Interfaces;
using Ordering.Infrastructure.Persistence;

namespace Ordering.Infrastructure.Repositories;

public class Repository<T> : IRepository<T> where T : BaseEntity
{
    private readonly OrderingDbContext _context;
    private readonly DbSet<T> _set;

    public Repository(OrderingDbContext context)
    {
        _context = context;
        _set = context.Set<T>();
    }

    public async Task<T?> GetByIdAsync(int id) => await _set.FindAsync(id);
    public async Task<IReadOnlyList<T>> GetAllAsync() => await _set.ToListAsync();
    public async Task AddAsync(T entity) => await _set.AddAsync(entity);
    public void Update(T entity) => _set.Update(entity);
    public void Remove(T entity) => _set.Remove(entity);
    public IQueryable<T> Query() => _set.AsQueryable();
}
