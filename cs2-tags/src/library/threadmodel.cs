using CounterStrikeSharp.API;

namespace Tags;

public static class CSSThread
{
    public static void RunOnMainThread(Action callback)
    {
        using SyncContextScope synchronizationContext = new();
        callback.Invoke();
    }

    public static async Task RunOnMainThreadAsync(Func<Task> callback)
    {
        await new Func<Task>(async () =>
        {
            using SyncContextScope synchronizationContext = new();
            await callback.Invoke();
        }).Invoke();
    }
}

public class SourceSynchronizationContext : SynchronizationContext
{
    public override void Post(SendOrPostCallback callback, object? state)
    {
        Server.NextWorldUpdate(() => callback(state));
    }

    public override SynchronizationContext CreateCopy()
    {
        return this;
    }
}

public class SyncContextScope : IDisposable
{
    private static readonly SynchronizationContext _sourceContext = new SourceSynchronizationContext();
    private readonly SynchronizationContext? _oldContext;
    private bool _disposed = false;

    public SyncContextScope()
    {
        _oldContext = SynchronizationContext.Current;
        SynchronizationContext.SetSynchronizationContext(_sourceContext);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                if (_oldContext != null)
                {
                    SynchronizationContext.SetSynchronizationContext(_oldContext);
                }
            }

            _disposed = true;
        }
    }
}