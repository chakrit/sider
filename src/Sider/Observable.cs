
using System;

namespace Sider
{
  internal class Observable<T> : IObservable<T>, IDisposable
  {
    private bool _disposed;
    private object _lock = new object();

    private event Action<T> OnNext;
    private event Action<Exception> OnError;
    private event Action OnCompleted;


    public void Next(T obj) { if (OnNext != null) OnNext(obj); }
    public void Error(Exception ex) { if (OnError != null) OnError(ex); }

    public void Complete()
    {
      if (OnCompleted == null) return;

      OnCompleted();
      Dispose();
    }


    public IDisposable Subscribe(IObserver<T> observer)
    {
      if (_disposed)
        throw new ObjectDisposedException("Observable has been disposed.");

      lock (_lock) {
        OnNext += observer.OnNext;
        OnError += observer.OnError;
        OnCompleted += observer.OnCompleted;
        return new DisposableDelegate(() =>
        {
          OnNext -= observer.OnNext;
          OnError -= observer.OnError;
          OnCompleted -= observer.OnCompleted;
        });
      }
    }


    public void Dispose()
    {
      if (_disposed) return;
      _disposed = true;

      OnNext = null;
      OnError = null;
      OnCompleted = null;
    }


    private class DisposableDelegate : IDisposable
    {
      private Action _disposeAction;

      public DisposableDelegate(Action disposeAction)
      {
        _disposeAction = disposeAction;
      }

      public void Dispose()
      {
        _disposeAction();
      }
    }

  }
}
