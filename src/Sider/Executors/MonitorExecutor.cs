
using System;
using System.Threading;

namespace Sider.Executors
{
  internal class MonitorExecutor : ExecutorBase
  {
    private Observable<string> _observable;
    
    
    public override Mode Mode { get { return Mode.Monitor;  } }

    public override T Execute<T>(Invocation<T> invocation)
    {
      if (invocation.Command == "QUIT") {
        // QUIT issued by the observable automatically
        _observable.Dispose();
        Client.Dispose();
        return default(T);
      }

      throw new InvalidOperationException(
        "Only QUIT for terminating MONITOR is allowed.");
    }

    public IObservable<string> BuildObservable()
    {
      return _observable = new MonitorObservable(this);
    }


    private class MonitorObservable : Observable<string>
    {
      private MonitorExecutor _parent;

      public MonitorObservable(MonitorExecutor parent)
      {
        _parent = parent;
        ThreadPool.QueueUserWorkItem(monitorThread);
      }

      private void monitorThread(object _)
      {
        try {
          while (!IsDisposed)
            Next(_parent.Reader.ReadStatus());

          Complete();
        }
        catch (Exception e) { Error(e); }
      }


      public override void Dispose()
      {
        _parent.ExecuteImmediate(Invocation.New<object>("QUIT", w => { }, r => null));
      }
    }
  }
}
