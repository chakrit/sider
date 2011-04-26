
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sider.Executors
{
  // TODO: Add HandleException support
  public class PipelinedExecutor : ExecutorBase
  {
    private Queue<Func<object>> _readsQueue;


    protected Queue<Func<object>> ReadsQueue { get { return _readsQueue; } }

    public override void Init(IExecutor previous)
    {
      if (previous is PipelinedExecutor)
        throw new InvalidOperationException("Already pipelining.");

      base.Init(previous);
      _readsQueue = new Queue<Func<object>>();
    }


    public override T Execute<T>(Invocation<T> invocation)
    {
      invocation.WriteAction(Writer);
      _readsQueue.Enqueue(() => invocation.ReadAction(Reader));

      return default(T);
    }

    public virtual IEnumerable<object> Finish()
    {
      return finishCore().ToArray();
    }

    private IEnumerable<object> finishCore()
    {
      Writer.Flush();
      while (_readsQueue.Count > 0)
        yield return _readsQueue.Dequeue()();
    }
  }
}
