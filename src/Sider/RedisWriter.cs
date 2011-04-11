
using System;
using System.IO;
using System.Text;

namespace Sider
{
  public class RedisWriter
  {
    private RedisSettings _settings;
    private Stream _stream;

    private byte[] _strBuffer;


    internal bool AutoFlush { get; set; }

    public RedisWriter(Stream stream) : this(stream, new RedisSettings()) { }

    public RedisWriter(Stream stream, RedisSettings settings)
    {
      SAssert.ArgumentNotNull(() => stream);
      SAssert.ArgumentSatisfy(() => stream, s => s.CanWrite, "Stream must be writable.");
      SAssert.ArgumentNotNull(() => settings);

      AutoFlush = false;

      _settings = settings;
      _stream = new BufferedStream(stream, _settings.WriteBufferSize);
      _strBuffer = new byte[_settings.StringBufferSize];
    }


    public void WriteLine(string str)
    {
      SAssert.ArgumentNotNull(() => str);

      var bytesNeeded = Encoding.Default.GetByteCount(str);
      if (bytesNeeded < _strBuffer.Length) {
        Encoding.Default.GetBytes(str, 0, str.Length, _strBuffer, 0);
        _stream.Write(_strBuffer, 0, bytesNeeded);
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
      _stream.WriteByte((byte)type);
      flushIfAuto();
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

      _stream.Write(buffer, offset, count);
      writeCrLf();
    }

    public void WriteBulkFrom(Stream source, int count)
    {
      SAssert.ArgumentNotNull(() => source);
      SAssert.ArgumentNonNegative(() => count);

      using (var limiter = new LimitingStream(source, count)) {
        limiter.CopyTo(_stream);
        writeCrLf();

        SAssert.IsTrue(limiter.BytesLeft == 0,
          () => new InvalidOperationException("Stream does not contains enough data."));
      }
    }


    public void Flush()
    {
      _stream.Flush();
    }

    private void flushIfAuto()
    {
      if (AutoFlush) Flush();
    }


    private void writeCrLf()
    {
      _stream.WriteByte(0x0D);
      _stream.WriteByte(0x0A);

      flushIfAuto();
    }

  }
}
