
using System;
using System.Collections.Generic;

namespace Sider.Executors
{
  internal class TransactedExecutor : PipelinedExecutor
  {
    public override void Init(IExecutor previous)
    {
      if (previous is TransactedExecutor)
        throw new InvalidOperationException("Cannot nest MULTI/EXEC.");

      // TODO: Do a really thorough test that this won't randomly break\
      // TODO: Maybe shouldn't really auto-pipeline by default because
      //   some commands could throw an error and it would be hard to debug
      //   if the error only happens when .Finish() (EXEC) is called.
      //   This shouldn't be the case with manual piplining because the scope
      //   is explicitly stated by the user.
      if (previous is PipelinedExecutor)
        throw new InvalidOperationException(
          "MULTI/EXEC is automatically pipelined.");

      base.Init(previous);
    }


    public override T Execute<T>(Invocation<T> invocation)
    {
      // TODO: Provide a settings to pipeline MultiExec automatically
      return base.Execute(invocation); // issue write and queue reads
    }

    public override IEnumerable<object> Finish()
    {
      Writer.WriteCmdStart("EXEC", 0);
      Writer.Flush();

      // reads out an equal number of +QUEUEDs beforehand
      // TODO: May need to restart if a read doesn't return an expected +QUEUEDs
      //   which means *some* commands may have been improperly sent
      //   may need to issue a DISCARD and start over.
      for (int i = 0; i < ReadsQueue.Count; i++)
        Reader.ReadQueued();

      Reader.ReadMultiBulkHeader(); // read-out a multi-bulk header
      return base.Finish(); // read out all the queued reads
    }

    public void Discard()
    {
      ReadsQueue.Clear();
    }
  }
}
