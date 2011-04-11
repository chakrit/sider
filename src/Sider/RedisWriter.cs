
using System;
using System.IO;
using System.Text;

namespace Sider
{
  public class RedisWriter
  {
    private Stream _stream;
    private byte[] _buffer;
    private int _bufferOffset;

    private RedisSettings _settings;


    internal bool AutoFlush { get; set; }

    public RedisWriter(Stream stream) : this(stream, new RedisSettings()) { }

    public RedisWriter(Stream stream, RedisSettings settings)
    {
      SAssert.ArgumentNotNull(() => stream);
      SAssert.ArgumentSatisfy(() => stream, s => s.CanWrite, "Stream must be writable.");
      SAssert.ArgumentNotNull(() => settings);

      _settings = settings;

      _stream = stream;
      _buffer = new byte[_settings.WriteBufferSize]; // use BufferredStream?
      _bufferOffset = 0;
    }


    public void WriteLine(string str)
    {
      SAssert.ArgumentNotNull(() => str);

      var bytesNeeded = Encoding.Default.GetByteCount(str) + 2;

      if (fitInBuffer(bytesNeeded)) {
        _bufferOffset += Encoding.Default
          .GetBytes(str, 0, str.Length, _buffer, _bufferOffset);
      }
      else {
        var buffer = Encoding.Default.GetBytes(str);
        _stream.Write(buffer, 0, buffer.Length);
      }

      writeCrLf();
    }

    public void WriteLine(int num)
    {
      // TODO: Optimize number translation?
      WriteLine(num.ToString());
    }


    public void WriteTypeChar(ResponseType type)
    {
      SAssert.ArgumentSatisfy(() => type,
        v => Enum.IsDefined(typeof(ResponseType), v), "Invalid type char.");

      // assuming writebuffersize >= 1 so fitInBuffer(1) == true
      // the else case is for testing purpose
      if (fitInBuffer(1))
        _buffer[_bufferOffset++] = (byte)type;
      else
        _stream.WriteByte((byte)type);
    }


    public void WriteBulk(byte[] buffer)
    {
      SAssert.ArgumentNotNull(() => buffer);

      WriteBulk(buffer, 0, buffer.Length);
    }

    public void WriteBulk(byte[] buffer, int offset, int count)
    {
      SAssert.ArgumentNotNull(() => buffer);

      if (!(offset == 0 && count == 0)) {
        SAssert.ArgumentBetween(() => offset, 0, buffer.Length);
        SAssert.ArgumentBetween(() => count, 0, buffer.Length + 1);
      }

      SAssert.ArgumentSatisfy(() => offset, o => o + count <= buffer.Length,
        "Offset plus count is larger than the buffer.");

      if (fitInBuffer(count)) {
        Buffer.BlockCopy(buffer, offset, _buffer, _bufferOffset, count);
        _bufferOffset += count;
      }
      else {
        _stream.Write(buffer, offset, count);
      }

      writeCrLf();
    }

    public void WriteBulkFrom(Stream source, int count)
    {
      SAssert.ArgumentNotNull(() => source);
      SAssert.ArgumentNonNegative(() => count);

      var bytesLeft = count;
      var chunkSize = 0;
      var bytesRead = 0;

      // absorb user-supplied stream read exceptions
      // to maintain valid writer state and then
      // rethrow when we've properly written out all the garbled bytes
      // RedisReader.ReadBulkTo should looks about the same
      using (var wrapper = new AbsorbingStreamWrapper(source)) {
        while (bytesLeft > 0) {
          chunkSize = fitInBuffer(bytesLeft) ?
            bytesLeft :
            Math.Min(bytesLeft, _buffer.Length);

          bytesRead = wrapper.Read(_buffer, _bufferOffset, chunkSize);
          SAssert.IsTrue(bytesRead > 0,
            () => new InvalidOperationException("Stream does not contains enough data."));

          bytesLeft -= bytesRead;
          _bufferOffset += bytesRead;
        }

        writeCrLf();
        wrapper.ThrowIfError();
      }
    }


    public void Flush()
    {
      _stream.Write(_buffer, 0, _bufferOffset);
      _stream.Flush();
      _bufferOffset = 0;
    }


    private bool fitInBuffer(int bytesNeeded)
    {
      // don't use the buffer in AutoFlush mode, just write
      // everything to the stream directly
      if (AutoFlush) {
        if (_bufferOffset > 0)
          Flush();

        return false;
      }

      // AutoFlush == false , buffer writes
      if (_bufferOffset + bytesNeeded > _buffer.Length) {
        // not enough space, try flushing first
        Flush();
        return bytesNeeded <= _buffer.Length;
      }

      // enough space is available
      return true;
    }

    private void writeCrLf()
    {
      if (fitInBuffer(2)) {
        _buffer[_bufferOffset++] = 0x0D;
        _buffer[_bufferOffset++] = 0x0A;
      }
      else {
        // should be rare case where writebuffersize < 2
        _stream.Write(new byte[] { 0x0D, 0x0A }, 0, 2);
      }
    }

  }
}
