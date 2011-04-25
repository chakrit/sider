
using System;
using System.IO;
using System.Net.Sockets;

namespace Sider
{
  public partial class RedisClient<T>
  {
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
      return _executor.Execute(Invocation.New(command, w =>
      {
        w.WriteCmdStart(command, numArgs);
        writeArgsAction(w);
      }, r => readAction(r)));
    }


    private bool handleException(Exception ex)
    {
      if (!(ex is WriteException || ex is ReadException))
        return false;

      var innerEx = ex.InnerException;
      if (!(innerEx is IOException || ex is ObjectDisposedException ||
        ex is SocketException))
        return false;

      // assumes Redis disconnected us due to timeouts
      Dispose();
      if (!Settings.ReconnectOnIdle)
        throw new TimeoutException(
          "Disconnection detected. Probably due to idle timeout.", ex);

      Reset();
      return true;
    }
  }
}
