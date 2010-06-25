
using System;
using System.Threading;
using System.Diagnostics;

namespace Sider
{
  [DebuggerStepThrough]
  internal class SiderAsyncResult<T> : IAsyncResult, IDisposable
  {
    private ManualResetEvent _waitHandle;

    public object AsyncState { get; set; }

    public WaitHandle AsyncWaitHandle
    {
      get { return _waitHandle = _waitHandle ?? new ManualResetEvent(false); }
    }

    public bool IsCompleted { get; private set; }
    public T Result { get; private set; }


    public bool CompletedSynchronously { get { throw new NotSupportedException(); } }


    public SiderAsyncResult(object asyncState = null)
    {
      Result = default(T);
      IsCompleted = false;
      AsyncState = asyncState;
    }


    /// <summary>
    /// Sets the result of this IAsyncResult object and set IsCompleted = true;
    /// </summary>
    public void SetResult(T result)
    {
      if (_waitHandle != null)
        _waitHandle.Set();

      IsCompleted = true;
      Result = result;
    }


    /// <summary>
    /// Creates a delegating callback that automatically calls SetResult() using
    /// the supplied asynchronous EndXXX function and invoke the user's provided
    /// callback.
    /// </summary>
    public AsyncCallback MakeAsyncCallback(Func<IAsyncResult, T> endFunc,
      AsyncCallback userCallback)
    {
      return ar =>
      {
        SetResult(endFunc(ar));
        userCallback(this);
      };
    }

    /// <summary>
    /// Creates a simple delegate callback for passing into another function to
    /// automatically call SetResult() and the user-supplied AsyncCallback.
    /// </summary>
    public Action<T> MakeSimpleCallback(AsyncCallback userCallback)
    {
      return result =>
      {
        SetResult(result);
        userCallback(this);
      };
    }


    public void Dispose()
    {
      if (_waitHandle != null)
        _waitHandle.Dispose();
    }
  }
}
