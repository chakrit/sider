
using System;
using System.Collections.Generic;

namespace Sider.Executors
{
  public class PipelinedExecutor : ExecutorBase
  {
    private Queue<Func<object>> _readsQueue;


    protected Queue<Func<object>> ReadsQueue { get { return _readsQueue; } }

    public PipelinedExecutor(IExecutor another) :
      base(another)
    {
      if (another is PipelinedExecutor)
        throw new InvalidOperationException("Already piplining.");

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
      Writer.Flush();
      while (_readsQueue.Count > 0)
        yield return _readsQueue.Dequeue()();
    }
  }
}
