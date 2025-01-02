using Microsoft.EntityFrameworkCore.Storage;

namespace Hotel_Core.ServiceContracts;

public interface IUnitOfWork
{
    Task<IDbContextTransaction> BeginTransactionAsync();
    Task SaveChangesAsync();
}