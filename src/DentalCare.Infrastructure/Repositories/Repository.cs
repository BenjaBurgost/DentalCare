using System.Linq.Expressions;
using DentalCare.Domain.Interfaces;
using DentalCare.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DentalCare.Infrastructure.Repositories;

/// <summary>Implementación genérica de IRepository sobre EF Core.</summary>
public class Repository<T> : IRepository<T> where T : class
{
    protected readonly DentalCareDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public Repository(DentalCareDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public virtual async Task<T?> GetByIdAsync(int id, CancellationToken ct = default)
        => await _dbSet.FindAsync(new object[] { id }, ct);

    public virtual async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken ct = default)
        => await _dbSet.AsNoTracking().ToListAsync(ct);

    public virtual async Task<IReadOnlyList<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default)
        => await _dbSet.AsNoTracking().Where(predicate).ToListAsync(ct);

    public virtual async Task AddAsync(T entity, CancellationToken ct = default)
        => await _dbSet.AddAsync(entity, ct);

    public virtual void Update(T entity) => _dbSet.Update(entity);

    public virtual void Remove(T entity) => _dbSet.Remove(entity);

    public virtual Task<int> SaveChangesAsync(CancellationToken ct = default)
        => _context.SaveChangesAsync(ct);
}
