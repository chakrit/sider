
using System;
using System.IO;
using System.Net.Sockets;

namespace Sider
{
  // TODO: May needs to convert this whole file into a class of its own
  //   an IExecutor with implementations for immediate/pipeline/transact/observe
  //   execution mode switched to and fro instead of using boolean switches.
  //   May make it easier to add something like an IOCP later as well.
  public partial class RedisClient<T>
  {
    private void execute(Action action)
    {
      execute<object>(() => { action(); return null; });
    }

    private TOut execute<TOut>(Func<TOut> func, int retryCount = 0)
    {
      try { return func(); }
      catch (Exception ex) {
        if (_isPipelining || !handleException(ex))
          throw;

        // acceptable exception processed by this point
        // retry if allowed to -- usually harmless since if there's a
        // disconnection it usually means nothing was processed on Redis side.
        if (_settings.ReissueCommandsOnReconnect &&
          retryCount < _settings.MaxReconnectRetries)
          return execute(func, retryCount + 1);

        // signal the client that the action failed or retried too many times
        // even if we've reconnected
        throw;
      }
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
      if (!_settings.ReconnectOnIdle)
        throw new TimeoutException(
          "Disconnection detected. Probably due to idle timeout.", ex);

      Reset();
      return true;
    }


    private void writeCore(Action<RedisWriter> writeAction)
    {
      ensureNotDisposed();

      try {
        // TODO: Add logging
        writeAction(_writer);

        // delay flushes when piplining
        if (!_isPipelining)
          _writer.Flush();
      }
      catch (Exception ex) {
        throw new WriteException(ex);
      }
    }

    private TOut readCore<TOut>(ResponseType expectedType,
      Func<RedisReader, TOut> readFunc)
    {
      ensureNotDisposed();

      try {
        // queue up reads if pipelining, otherwise just reads immediately
        if (!_isPipelining) {
          // TODO: Add logging
          // TODO: Add error-checking support to reads
          var type = _reader.ReadTypeChar();
          SAssert.ResponseType(expectedType, type, _reader);

          return readFunc(_reader);
        }

        // piplining or inside transaction, queue up the reads
        _readsQueue.Enqueue(r =>
        {
          // TODO: What to do with exceptions?
          var type = r.ReadTypeChar();
          SAssert.ResponseType(expectedType, type, r);

          return readFunc(r);
        });

        return default(TOut);

      }
      catch (Exception ex) {
        throw new ReadException(ex);
      }
    }
  }
}
