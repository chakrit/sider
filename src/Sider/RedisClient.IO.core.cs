
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;

namespace Sider
{
  public partial class RedisClient<T>
  {
    private bool _isPipelining;
    private Queue<Func<RedisReader, object>> _readsQueue;


    private IEnumerable<object> pipelineCore(Action<IRedisClient<T>> pipelinedCalls)
    {
      _readsQueue = _readsQueue ?? new Queue<Func<RedisReader, object>>();

      // all writes executed immediately but reads are queued
      _isPipelining = true;
      pipelinedCalls(this);
      _writer.Flush();
      _isPipelining = false;


      // reads out all the return values
      while (_readsQueue.Count > 0)
        yield return _readsQueue.Dequeue()(_reader);
    }


    private void execute(Action action)
    {
      execute<object>(() => { action(); return null; });
    }

    private TOut execute<TOut>(Func<TOut> func, int retryCount = 0)
    {
      try { return func(); }
      catch (Exception ex) {
        if (!handleException(ex))
          throw;

        // exception processed by this point
        if ((_settings.ReissueWriteOnReconnect && ex is WriteException) ||
          (_settings.ReissueReadOnReconnect && ex is ReadException)) {

          // retry once, if allowed to -- usually harmless even with writes
          // since if there's a disconnection it should means nothing was processed.
          if (retryCount < _settings.MaxReconnectRetries)
            return execute(func, retryCount + 1);

          // no more retries
          throw;
        }

        // signal the client that the action failed.
        // (even though we've reconnected)
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
        throw timeoutException(ex);

      Reset();
      return true;
    }

    private Exception timeoutException(Exception inner)
    {
      return new TimeoutException(
        "Disconnection detected. Probably due to idle timeout.", inner);
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
        else {
          _readsQueue.Enqueue(r =>
          {
            var type = r.ReadTypeChar();
            SAssert.ResponseType(expectedType, type, r);

            return readFunc(r);
          });

          return default(TOut);
        }

      }
      catch (Exception ex) {
        throw new ReadException(ex);
      }
    }
  }
}
