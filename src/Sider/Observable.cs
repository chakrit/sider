
using System;

namespace Sider
{
  internal abstract class Observable<T> : IObservable<T>, IDisposable
  {
    protected event Action<T> OnNext;
    protected event Action<Exception> OnError;
    protected event Action OnCompleted;

    private int _subscribers;
    private bool _disposed;
    private object _lock;


    public bool IsDisposed { get { return _disposed; } }
    public int SubscribersCount { get { return _subscribers; } }

    public Observable()
    {
      _disposed = false;
      _lock = new object();
    }


    protected void Next(T obj) { if (OnNext != null) OnNext(obj); }
    protected void Error(Exception ex) { if (OnError != null) OnError(ex); }
    protected void Complete() { if (OnCompleted != null) OnCompleted(); }

    public IDisposable Subscribe(IObserver<T> observer)
    {
      if (_disposed)
        throw new ObjectDisposedException("Observable has been disposed.");

      lock (_lock) {
        _subscribers++;
        OnNext += observer.OnNext;
        OnError += observer.OnError;
        OnCompleted += observer.OnCompleted;
        return new DisposableDelegate(() =>
        {
          lock (_lock) {
            OnNext -= observer.OnNext;
            OnError -= observer.OnError;
            OnCompleted -= observer.OnCompleted;
            _subscribers--;
          }
        });
      }
    }


    public virtual void Dispose()
    {
      if (_disposed) return;
      _disposed = true;
      _subscribers = 0;

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
