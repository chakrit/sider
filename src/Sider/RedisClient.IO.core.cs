
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


    private IEnumerable<object> executePipeline(Action<IRedisClient<T>> pipelinedCalls,
      int retryCount = 0)
    {
      object[] results;

      try {
        _readsQueue = _readsQueue ?? new Queue<Func<RedisReader, object>>();
        _readsQueue.Clear();

        // all writes executed immediately but reads are queued
        _isPipelining = true;
        pipelinedCalls(this);
        _writer.Flush();
        _isPipelining = false;


        // reads out all the return values
        results = new object[_readsQueue.Count];
        var resultIdx = 0;

        while (_readsQueue.Count > 0)
          results[resultIdx++] = _readsQueue.Dequeue()(_reader);
      }
      catch (Exception ex) {
        if (!handleException(ex))
          throw;

        // TODO: multiple retries with pipeline maybe too dangerous
        //   so will only retry once in pipeline mode.
        if (_settings.ReissuePipelinedCallsOnReconnect &&
          retryCount < 1)
          return executePipeline(pipelinedCalls, retryCount + 1);

        throw;
      }

      return results;
    }


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
