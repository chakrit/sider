
using System;
using System.Collections.Generic;

namespace Sider.Executors
{
  public class TransactedExecutor : PipelinedExecutor
  {
    private IExecutor _executor;

    public TransactedExecutor(IExecutor another) :
      base(another)
    {
      if (another is TransactedExecutor)
        throw new InvalidOperationException("Cannot nest MULTI/EXEC.");

      // TODO: Make possible.
      if (another is PipelinedExecutor)
        throw new InvalidOperationException(
          "MULTI/EXEC cannot be called inside .Pipeline");
    }


    public override T Execute<T>(Invocation<T> invocation)
    {
      // TODO: Provide a settings to pipeline MultiExec automatically
      var result = base.Execute(invocation); // invocation.Read should be queued
      Writer.Flush(); // should flush? or wait for pipeline?
      Reader.ReadQueued(); // read a +QUEUED

      return result;
    }

    public override IEnumerable<object> Finish()
    {
      Writer.WriteCmdStart("EXEC", 0);
      return base.Finish();
    }

    public void Discard()
    {
      ReadsQueue.Clear();
    }
  }
}
