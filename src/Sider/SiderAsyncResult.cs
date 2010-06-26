
using System;
using System.Diagnostics;
using System.Threading;

namespace Sider
{
  // TODO: Add support for (void) return value
  // TODO: Add support for attached IAsyncResult
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


    public void SetResult(T result)
    {
      if (_waitHandle != null)
        _waitHandle.Set();

      IsCompleted = true;
      Result = result;
    }


    public AsyncCallback MakeAsyncCallback(Func<IAsyncResult, T> endFunc,
      AsyncCallback userCallback)
    {
      return ar =>
      {
        SetResult(endFunc(ar));
        userCallback(this);
      };
    }

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
