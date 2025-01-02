using System.Reflection;
using Hotel_Core.ServiceContracts;

namespace Hotel_Core.Services;

public class TransactionDecorator<TService> : DispatchProxy
{
    private TService _decorated;
    private IUnitOfWork _unitOfWork;

    public static TService Create(TService decorated, IUnitOfWork unitOfWork)
    {
        object proxy = Create<TService, TransactionDecorator<TService>>();
        ((TransactionDecorator<TService>)proxy)._decorated = decorated;
        ((TransactionDecorator<TService>)proxy)._unitOfWork = unitOfWork;
        return (TService)proxy;
    }

    protected override object Invoke(MethodInfo targetMethod, object[] args)
    {
        using var transaction = _unitOfWork.BeginTransactionAsync().Result;
        try
        {
            var result = targetMethod.Invoke(_decorated, args);
            _unitOfWork.SaveChangesAsync().Wait();
            transaction.CommitAsync().Wait();
            return result;
        }
        catch
        {
            transaction.RollbackAsync().Wait();
            throw;
        }
    }
}