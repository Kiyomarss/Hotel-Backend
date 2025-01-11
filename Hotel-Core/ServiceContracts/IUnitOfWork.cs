namespace Hotel_Core.ServiceContracts;

public interface IUnitOfWork
{
    Task BeginTransactionAsync();
    
    Task CommitTransactionAsync();
    
    Task RollbackTransactionAsync();
    
    Task<int> SaveChangesAsync();
}