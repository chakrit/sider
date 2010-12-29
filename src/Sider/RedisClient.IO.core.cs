
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace Sider
{
  public partial class RedisClient
  {
    private bool _isPipelining;
    private Queue<Func<RedisReader, object>> _readsQueue;


    public IEnumerable<object> Pipeline(Action<IRedisClient> pipelinedCalls)
    {
      _readsQueue = _readsQueue ?? new Queue<Func<RedisReader, object>>();

      // all writes executed immediately but reads are queued
      _isPipelining = true;
      pipelinedCalls(this);
      _isPipelining = false;

      // reads out all the return values
      while (_readsQueue.Count > 0)
        yield return _readsQueue.Dequeue()(_reader);
    }


    private void writeCore(Action<RedisWriter> writeAction)
    {
      ensureNotDisposed();

      try {
        // TODO: Add logging
        writeAction(_writer);

      }
      catch (Exception ex) {

        ensureClientState(ex);
        if (_disposed)
          throw;

        // usually, this catch block is run because of
        // idle connection timeouts from Redis side
        if (!_settings.ReconnectOnIdle)
          Dispose();

        // try again one more time before giving up
        // TODO: havn't encountered any issues with re-issuing commands since
        //   failed writes usually means the data remains untouched on the
        //   redis side but there should be a settings to prevent this.
        try {
          Reset();
          writeAction(_writer);
        }
        catch (Exception ex_) {
          ensureClientState(ex_);
          throw;
        }
      }
    }

    private T readCore<T>(ResponseType expectedType, Func<RedisReader, T> readFunc)
    {
      ensureNotDisposed();

      try {
        // queue up reads if pipelining, otherwise just reads immediately
        if (!_isPipelining) {
          // TODO: Add logging
          // TODO: Add error-checking support to reads
          var type = _reader.ReadTypeChar();
          SAssert.ResponseType(expectedType, type);

          return readFunc(_reader);
        }
        else {
          _readsQueue.Enqueue(r =>
          {
            var type = r.ReadTypeChar();
            SAssert.ResponseType(expectedType, type);

            return readFunc(r);
          });

          return default(T);
        }

      }
      catch (Exception ex) {
        ensureClientState(ex);
        throw;
      }
    }


    private void ensureClientState(Exception ex)
    {
      // TODO: Absorbing a generic IOException might be too dangerous.
      //       Multibulk operations may still cause the reader/writer into
      //       invalid states. e.g. First reads error, then absorbed, then
      //       other required bulk read skipped (but client is not disposed
      //       so user may issue more bulk commands and encounter parsing
      //       exceptions)
      if (!(ex is IOException || ex is ObjectDisposedException))
        Dispose();
    }
  }
}
