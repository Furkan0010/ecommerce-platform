using Payment.Domain.Common;
using Payment.Domain.Interfaces;
using Payment.Infrastructure.Persistence;

namespace Payment.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly PaymentDbContext _context;
    private readonly Dictionary<Type, object> _repositories = new();

    public UnitOfWork(PaymentDbContext context) => _context = context;

    public IRepository<T> Repository<T>() where T : BaseEntity
    {
        if (_repositories.TryGetValue(typeof(T), out var existing))
            return (IRepository<T>)existing;

        var repository = new Repository<T>(_context);
        _repositories[typeof(T)] = repository;
        return repository;
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => _context.SaveChangesAsync(cancellationToken);
}
