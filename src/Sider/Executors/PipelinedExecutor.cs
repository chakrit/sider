
using System;
using System.Collections.Generic;

namespace Sider.Executors
{
  public class PipelinedExecutor : ExecutorBase
  {
    private Queue<Func<object>> _readsQueue;
    private IEnumerable<object> _result;


    public IEnumerable<object> Result { get { return _result; } }

    public PipelinedExecutor(RedisSettings settings,
      ProtocolReader reader, ProtocolWriter writer) :
      base(settings, reader, writer)
    {
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
      while (_readsQueue.Count > 0)
        yield return _readsQueue.Dequeue()();
    }
  }
}
