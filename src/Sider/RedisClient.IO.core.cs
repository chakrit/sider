
using System;

namespace Sider
{
  public partial class RedisClient<T>
  {
    private void invoke(string command)
    {
      invoke(command, r => (object)null);
    }

    private TInv invoke<TInv>(string command,
      Func<ProtocolReader, TInv> readAction)
    {
      return invoke<TInv>(command, 0, w => { }, readAction);
    }

    private TInv invoke<TInv>(string command, string key,
      Func<ProtocolReader, TInv> readAction)
    {
      return invoke<TInv>(command, 1, w => w.WriteArg(key), readAction);
    }

    private TInv invoke<TInv>(string command, string key, T value,
      Func<ProtocolReader, TInv> readAction)
    {
      return invoke<TInv>(command, 2,
        w => { w.WriteArg(key); w.WriteArg(_serializer, value); },
        readAction);
    }

    private TInv invoke<TInv>(
      string command, int numArgs,
      Action<ProtocolWriter> writeArgsAction,
      Func<ProtocolReader, TInv> readAction)
    {
      return invoke(Invocation.New(command, w =>
      {
        w.WriteCmdStart(command, numArgs);
        writeArgsAction(w);
      }, r => readAction(r)));
    }

    private TInv invoke<TInv>(Invocation<TInv> invocation)
    {
      if (IsDisposed)
        throw new ObjectDisposedException(
          "RedisClient instance has been disposed and is no longer usable.\r\n" +
          "You may reconnect by issuing a .Reset().");

#if DEBUG
      if (_cmdInterceptor != null) {
        _cmdInterceptor(invocation.Command);
        return default(TInv);
      }
#endif

      return Executor.Execute(invocation);
    }


#if DEBUG
    // extra hooks for testing commands in batch
    private Action<string> _cmdInterceptor;

    internal void InterceptCommands(Action<string> commandFilter)
    {
      _cmdInterceptor = commandFilter;
    }
#endif
  }
}
