using System.Collections.Concurrent;

public sealed class WasapiThread : IDisposable
{
    private readonly Thread _thread;
    private readonly BlockingCollection<Action> _queue = new();

    public WasapiThread()
    {
        _thread = new Thread(ThreadLoop)
        {
            IsBackground = true,
            Name = "WASAPI Thread"
        };

        _thread.SetApartmentState(ApartmentState.STA);
        _thread.Start();
    }

    private void ThreadLoop()
    {
        foreach (var action in _queue.GetConsumingEnumerable())
            action();
    }

    public T Invoke<T>(Func<T> func)
    {
        T result = default;
        Exception error = null;
        var done = new ManualResetEventSlim();

        _queue.Add(() =>
        {
            try { result = func(); }
            catch (Exception ex) { error = ex; }
            finally { done.Set(); }
        });

        done.Wait();

        if (error != null)
            throw error;

        return result;
    }

    public void Dispose()
    {
        _queue.CompleteAdding();
    }
}
