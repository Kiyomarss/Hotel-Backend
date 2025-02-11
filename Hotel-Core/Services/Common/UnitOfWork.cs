using Hotel_Core.ServiceContracts;
using Microsoft.Extensions.Logging;
using RepositoryContracts;
namespace Hotel_Core.Services;
using Microsoft.EntityFrameworkCore.Storage;

public class UnitOfWork : IUnitOfWork
{
    private readonly IApplicationDbContext _db;
    private IDbContextTransaction? _transaction;
    private readonly ILogger<UnitOfWork> _logger;


    public UnitOfWork(IApplicationDbContext db, ILogger<UnitOfWork> logger)
    {
        _db = db;
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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
    
    public async Task<T> ExecuteTransactionAsync<T>(Func<Task<T>> operation)
    {
        await BeginTransactionAsync();
        try
        {
            var result = await operation();
            await SaveChangesAsync();
            await CommitTransactionAsync();
            return result;
        }
        catch (Exception ex)
        {
            await RollbackTransactionAsync();
            _logger.LogError(ex, "Transaction failed.");

            throw;
        }
    }


    public void Dispose()
    {
        try
        {
            _transaction?.Dispose();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error disposing transaction: {ex.Message}");
        }
    
        _db.Dispose();
    }
}