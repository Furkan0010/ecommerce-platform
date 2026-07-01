using Ordering.Domain.Common;
using Ordering.Domain.Interfaces;

namespace Ordering.Tests;

public class FakeRepository<T> : IRepository<T> where T : BaseEntity
{
    public readonly List<T> Items = new();
    public Task<T?> GetByIdAsync(int id) => Task.FromResult(Items.FirstOrDefault(x => x.Id == id));
    public Task<IReadOnlyList<T>> GetAllAsync() => Task.FromResult((IReadOnlyList<T>)Items);
    public Task AddAsync(T entity) { Items.Add(entity); return Task.CompletedTask; }
    public void Update(T entity) { }
    public void Remove(T entity) => Items.Remove(entity);
    public IQueryable<T> Query() => Items.AsQueryable();
}

public class FakeUnitOfWork : IUnitOfWork
{
    private readonly Dictionary<Type, object> _repositories = new();

    public IRepository<T> Repository<T>() where T : BaseEntity
    {
        if (!_repositories.TryGetValue(typeof(T), out var repo))
        {
            repo = new FakeRepository<T>();
            _repositories[typeof(T)] = repo;
        }
        return (IRepository<T>)repo;
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => Task.FromResult(1);
}
