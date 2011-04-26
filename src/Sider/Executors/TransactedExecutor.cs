
using System;
using System.Collections.Generic;

namespace Sider.Executors
{
  public class TransactedExecutor : PipelinedExecutor
  {
    public override void Init(IExecutor previous)
    {
      if (previous is TransactedExecutor)
        throw new InvalidOperationException("Cannot nest MULTI/EXEC.");

      // TODO: Make possible.
      if (previous is PipelinedExecutor)
        throw new InvalidOperationException(
          "MULTI/EXEC cannot be called inside .Pipeline()");

      base.Init(previous);
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
