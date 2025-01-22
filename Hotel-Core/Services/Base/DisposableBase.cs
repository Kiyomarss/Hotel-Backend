namespace Hotel_Core.Services.Base;

public abstract class DisposableBase : IDisposable, IAsyncDisposable
{
    private bool _disposed = false;

    protected abstract void DisposeManagedResources();
    protected virtual Task DisposeManagedResourcesAsync() => Task.CompletedTask;
    protected virtual void DisposeUnmanagedResources() { }

    public void Dispose()
    {
        if (!_disposed)
        {
            DisposeManagedResources();
            DisposeUnmanagedResources();
            _disposed = true;
        }
        GC.SuppressFinalize(this);
    }

    public async ValueTask DisposeAsync()
    {
        if (!_disposed)
        {
            await DisposeManagedResourcesAsync();
            DisposeUnmanagedResources();
            _disposed = true;
        }
        GC.SuppressFinalize(this);
    }
}