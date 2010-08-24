
using System;
using System.Diagnostics.Contracts;
using System.IO;

namespace Sider.Tests
{
  // a stream that allows to setup an exception to be thrown at a specific point
  // in the stream while reading/writing to test network error conditions
  public class TestExceptionStream : Stream
  {
    private Stream _underlying;

    private Exception _exception;
    private long _throwPosition;


    public TestExceptionStream() : this(new MemoryStream(), -1, null) { }
    public TestExceptionStream(Stream underlying) : this(underlying, -1, null) { }
    public TestExceptionStream(int throwingPosition, Exception exception) :
      this(new MemoryStream(), throwingPosition, exception) { }

    public TestExceptionStream(Stream underlying, int throwingPosition, Exception exception)
    {
      Contract.Requires(underlying != null, "Underlying stream cannot be null.");
      Contract.Requires(underlying.CanSeek, "Underlying stream must be seekable.");

      _underlying = underlying;

      _exception = null;
      _throwPosition = long.MaxValue;

      SetupException(throwingPosition, exception);
    }


    public void SetupException(long throwingPosition, Exception exception)
    {
      _exception = exception;
      _throwPosition = throwingPosition;
    }

    public void Reset()
    {
      _exception = null;
      _throwPosition = long.MaxValue;
    }

    protected void ThrowCheck(Action act)
    {
      act();

      if (_exception != null && _underlying.Position >= _throwPosition)
        throw _exception;
    }

    protected T ThrowCheck<T>(Func<T> func)
    {
      var result = func();

      if (_underlying.Position >= _throwPosition)
        throw _exception;

      return result;
    }


    protected override void Dispose(bool disposing)
    {
      if (true) _underlying.Dispose();

      base.Dispose(disposing);
    }


    #region Stream members

    public override bool CanRead { get { return _underlying.CanRead; } }
    public override bool CanWrite { get { return _underlying.CanWrite; } }
    public override bool CanSeek { get { return _underlying.CanSeek; } }

    public override long Length { get { return _underlying.Length; } }

    public override long Position
    {
      get { return _underlying.Position; }
      set { _underlying.Position = value; }
    }


    public override void Flush() { _underlying.Flush(); }

    public override long Seek(long offset, SeekOrigin origin)
    {
      return ThrowCheck(() => _underlying.Seek(offset, origin));
    }

    public override void SetLength(long value)
    {
      ThrowCheck(() => _underlying.SetLength(value));
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
      return ThrowCheck(() => _underlying.Read(buffer, offset, count));
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
      ThrowCheck(() => _underlying.Write(buffer, offset, count));
    }

    #endregion
  }
}
