
using System;
using System.IO;

namespace Sider
{
  internal class LimitingStream : Stream
  {
    private Stream _inner;
    private int _bytesLeft;


    public override bool CanRead { get { return true; } }
    public override bool CanSeek { get { return false; } }
    public override bool CanWrite { get { return false; } }

    public override long Length
    {
      get { return Math.Min(_bytesLeft, _inner.Length); }
    }

    public override long Position
    {
      get { return _inner.Position; }
      set { _inner.Position = value; }
    }

    public int BytesLeft { get { return _bytesLeft; } }

    public LimitingStream(Stream inner, int bytesLimit)
    {
      SAssert.ArgumentNotNull(() => inner);
      SAssert.ArgumentNonNegative(() => bytesLimit);
      SAssert.ArgumentSatisfy(() => inner, s => s.CanRead,
        "Stream must be readable.");

      _inner = inner;
      _bytesLeft = bytesLimit;
    }


    public override void Flush()
    {
      _inner.Flush();
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
      var chunkSize = Math.Min(count, _bytesLeft);
      var result = _inner.Read(buffer, offset, chunkSize);

      _bytesLeft -= result;
      return result;
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
      throw new NotSupportedException();
    }

    public override void SetLength(long value)
    {
      throw new NotSupportedException();
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
      throw new NotSupportedException();
    }
  }
}
