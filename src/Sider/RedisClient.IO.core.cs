
using System;
using System.IO;

namespace Sider
{
  // TODO: Detect invalid client state and self-dispose or restart
  //       e.g. when protocol errors occour
  public partial class RedisClient
  {
    private void writeCore(Action<RedisWriter> writeAction)
    {
      ensureNotDisposed();

      try {
        // TODO: Add pipelining support by recording writes
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
        // TODO: Add pipelining support by recording reads
        // TODO: Add logging
        // TODO: Add error-checking support to reads
        var type = _reader.ReadTypeChar();
        SAssert.ResponseType(expectedType, type);

        return readFunc(_reader);
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
