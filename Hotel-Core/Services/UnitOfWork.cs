using Hotel_Core.ServiceContracts;
using RepositoryContracts;
namespace Hotel_Core.Services;
using Microsoft.EntityFrameworkCore.Storage;

public class UnitOfWork : IUnitOfWork
{
    private readonly IApplicationDbContext _db;
    private IDbContextTransaction? _transaction;

    public UnitOfWork(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task BeginTransactionAsync()
    {
        if (_transaction != null) return;
        _transaction = await _db.Database.BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync()
    {
        if (_transaction == null)
        {
            throw new InvalidOperationException("No transaction has been started.");
        }

        await _transaction.CommitAsync();
        await _transaction.DisposeAsync();
        _transaction = null;
    }

    public async Task RollbackTransactionAsync()
    {
        if (_transaction == null) 
        {
            throw new InvalidOperationException("No transaction has been started.");
        }

        await _transaction.RollbackAsync();
        await _transaction.DisposeAsync();
        _transaction = null;
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _db.SaveChangesAsync();
    }
}
