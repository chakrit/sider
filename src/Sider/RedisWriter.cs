
using System.IO;
using System.Text;
using System;

namespace Sider
{
  public class RedisWriter
  {
    public const int DefaultBufferSize = 256;


    public static byte[] CrLfBuffer = new byte[] { 0x0D, 0x0A };


    private Stream _stream;
    private byte[] _buffer;

    public RedisWriter(Stream stream, int _bufferSize = 256)
    {
      Assert.ArgumentNotNull(() => stream);
      Assert.ArgumentPositive(() => _bufferSize);
      Assert.ArgumentSatisfy(() => stream, s => s.CanWrite, "Stream must be writable.");

      _stream = stream;
      _buffer = new byte[_bufferSize];
    }


    public void WriteLine(string str)
    {
      Assert.ArgumentNotNull(() => str);

      var bytesWrote = Encoding.Default.GetBytes(str, 0, str.Length, _buffer, 0);
      _stream.Write(_buffer, 0, bytesWrote);
      writeCrLf();
    }

    public void WriteLine(int num)
    {
      WriteLine(num.ToString());
    }


    public void WriteTypeChar(ResponseType type)
    {
      Assert.ArgumentSatisfy(() => type,
        v => Enum.IsDefined(typeof(ResponseType), v), "Invalid type char.");

      _stream.WriteByte((byte)type);
    }


    public void WriteBulk(byte[] buffer)
    {
      Assert.ArgumentNotNull(() => buffer);

      WriteBulk(buffer, 0, buffer.Length);
    }

    public void WriteBulk(byte[] buffer, int offset, int count)
    {
      Assert.ArgumentNotNull(() => buffer);

      if (!(offset == 0 && count == 0)) {
        Assert.ArgumentBetween(() => offset, 0, buffer.Length);
        Assert.ArgumentBetween(() => count, 0, buffer.Length + 1);
      }

      Assert.ArgumentSatisfy(() => offset, o => o + count <= buffer.Length,
        "Offset plus count is larger than the buffer.");

      _stream.Write(buffer, offset, count);
      writeCrLf();
    }

    public void WriteBulkFrom(Stream stream, int count)
    {
      Assert.ArgumentNotNull(() => stream);
      Assert.ArgumentNonNegative(() => count);

      var bytesLeft = count;

      while (bytesLeft > 0) {
        var chunkSize = bytesLeft > _buffer.Length ? _buffer.Length : bytesLeft;
        var bytesRead = stream.Read(_buffer, 0, chunkSize);

        Assert.IsTrue(bytesRead > 0,
          () => new InvalidOperationException("Stream does not contains enough data."));

        _stream.Write(_buffer, 0, bytesRead);
        bytesLeft -= bytesRead;
      }

      //Assert.IsTrue(bytesRead == count,
      //  () => new InvalidOperationException("Not enough data."));
      writeCrLf();
    }


    private void writeCrLf()
    {
      _stream.Write(CrLfBuffer, 0, CrLfBuffer.Length);
    }
  }
}
