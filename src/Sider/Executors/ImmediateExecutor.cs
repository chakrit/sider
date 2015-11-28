
using System;

namespace Sider.Executors
{
  internal class ImmediateExecutor : ExecutorBase
  {
    public override Mode Mode { get { return Mode.Normal; } }
    
    public override T Execute<T>(Invocation<T> invocation)
    {
      return execute(invocation, 0);
    }

    private T execute<T>(Invocation<T> invocation, int retryCount)
    {
      try { return ExecuteImmediate(invocation); }
      catch (Exception ex) {
        if (!HandleTimeout(ex))
          throw;

        // client/connection state ensured by this point
        // will throw timeout exception if not set to retry,
        // but client state remains usable despise the exception
        if (!Settings.ReissueCommandsOnReconnect ||
          retryCount >= Settings.MaxReconnectRetries)
          throw new IdleTimeoutException(ex);

        // retry
        return execute(invocation, retryCount + 1);
      }
    }
  }
}
