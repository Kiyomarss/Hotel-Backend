namespace Hotel_Core.ServiceContracts;

public interface IUnitOfWork : IDisposable
{
    Task BeginTransactionAsync();
    
    Task CommitTransactionAsync();
    
    Task RollbackTransactionAsync();
    
    Task<int> SaveChangesAsync();
}