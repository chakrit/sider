
using System;
using System.Threading;

namespace Sider.Executors
{
  public class MonitorExecutor : ExecutorBase
  {
    private Observable<string> _observable;

    public MonitorExecutor(IExecutor another) :
      base(another) { }


    public override T Execute<T>(Invocation<T> invocation)
    {
      if (invocation.Command == "QUIT") {
        Writer.WriteCmdStart("QUIT", 0);
        _observable.Dispose();
      }

      throw new InvalidOperationException(
        "Only QUIT for terminating MONITOR is allowed.");
    }

    public IObservable<string> BuildObservable()
    {
      return new MonitorObservable(this);
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
        while (true)
          Next(_parent.Reader.ReadStatus());
      }


      public override void Dispose()
      {
        _parent.Writer.WriteCmdStart("QUIT", 0);
        _parent.Writer.Flush();
      }
    }
  }
}
