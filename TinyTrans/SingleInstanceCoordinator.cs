using System.Windows.Threading;

namespace TinyTrans;

public sealed class SingleInstanceCoordinator : IDisposable
{
    private readonly string _mutexName;
    private readonly string _activateEventName;
    private readonly Dispatcher _dispatcher;
    private Mutex? _mutex;
    private EventWaitHandle? _activateEvent;
    private bool _disposed;

    public SingleInstanceCoordinator(string mutexName, string activateEventName, Dispatcher dispatcher)
    {
        _mutexName = mutexName;
        _activateEventName = activateEventName;
        _dispatcher = dispatcher;
    }

    public bool TryStart(Action activateCurrentInstance)
    {
        _mutex = new Mutex(true, _mutexName, out var createdNew);

        if (!createdNew)
        {
            SignalExistingInstance();
            return false;
        }

        _activateEvent = new EventWaitHandle(false, EventResetMode.AutoReset, _activateEventName);
        var activateThread = new Thread(() => ListenForActivation(activateCurrentInstance))
        {
            IsBackground = true,
            Name = "TinyTrans activation listener"
        };
        activateThread.Start();

        return true;
    }

    private void SignalExistingInstance()
    {
        using var signal = new EventWaitHandle(false, EventResetMode.AutoReset, _activateEventName);
        signal.Set();
    }

    private void ListenForActivation(Action activateCurrentInstance)
    {
        while (!_disposed)
        {
            try
            {
                _activateEvent?.WaitOne();
            }
            catch
            {
                break;
            }

            if (_disposed)
                break;

            _dispatcher.Invoke(activateCurrentInstance);
        }
    }

    public void Dispose()
    {
        _disposed = true;
        _activateEvent?.Dispose();
        _mutex?.Dispose();
    }
}
