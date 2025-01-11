namespace Hotel_Core.Services.Base;

public abstract class DisposableBase : IDisposable
{
    private bool _disposed = false;

    // این متد توسط کلاس‌های فرزند برای مدیریت منابع پیاده‌سازی می‌شود
    protected abstract void DisposeManagedResources();

    // مدیریت منابع غیرمدیریت‌شده (در صورت نیاز)
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

    // Finalizer
    ~DisposableBase()
    {
        Dispose();
    }
}