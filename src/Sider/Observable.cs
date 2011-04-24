
using System;

namespace Sider
{
  internal abstract class Observable<T> : IObservable<T>, IDisposable
  {
    private bool _disposed;
    private Action _disposeAction;
    private object _lock;

    private event Action<T> OnNext;
    private event Action<Exception> OnError;
    private event Action OnCompleted;


    public Observable(Action disposeAction)
    {
      _disposed = false;
      _disposeAction = disposeAction;
      _lock = new object();
    }


    protected void Next(T obj) { if (OnNext != null) OnNext(obj); }
    protected void Error(Exception ex) { if (OnError != null) OnError(ex); }

    protected void Complete()
    {
      if (OnCompleted != null)
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


    public virtual void Dispose()
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
