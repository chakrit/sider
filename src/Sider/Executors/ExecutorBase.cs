
using System;
using System.IO;
using System.Net.Sockets;

namespace Sider.Executors
{
  // TODO: Filter invalid commands
  internal abstract class ExecutorBase : IExecutor
  {
    public RedisClientBase Client { get; private set; }

    protected RedisSettings Settings { get { return Client.Settings; } }
    protected ProtocolReader Reader { get { return Client.Reader; } }
    protected ProtocolWriter Writer { get { return Client.Writer; } }


    public virtual void Init(RedisClientBase client)
    {
      SAssert.ArgumentNotNull(() => client);
      Client = client;
    }

    public virtual void Init(IExecutor previous)
    {
      SAssert.ArgumentNotNull(() => previous);
      Client = previous.Client;
    }


    public abstract T Execute<T>(Invocation<T> invocation);

    protected T ExecuteImmediate<T>(Invocation<T> invocation)
    {
      invocation.WriteAction(Writer);
      Writer.Flush();

      var result = invocation.ReadAction(Reader);

      // handle connection state changes after some commands
      if (invocation.Command == "QUIT" ||
        invocation.Command == "SHUTDOWN")
        Client.Dispose();

      return result;
    }


    protected bool HandleTimeout(Exception ex)
    {
      // assumes Redis disconnected (thus the IOException/SocketException)
      // due to idle timeout.
      if (!(ex is IOException || ex is ObjectDisposedException ||
        ex is SocketException))
        return false;

      Client.Dispose();
      if (!Settings.ReconnectOnIdle)
        throw new IdleTimeoutException(ex);

      // reconnect client
      Client.Reset();
      return true;
    }


    public virtual void Dispose() { }
  }
}
