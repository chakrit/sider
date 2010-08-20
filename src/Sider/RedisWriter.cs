
using System;
using System.IO;
using System.Text;

namespace Sider
{
  public class RedisWriter
  {
    public const int DefaultBufferSize = 4096;


    public static byte[] CrLfBuffer = new byte[] { 0x0D, 0x0A };


    private Stream _stream;
    private byte[] _buffer;

    public RedisWriter(Stream stream, int _bufferSize = DefaultBufferSize)
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

      // TODO: Account for strings larger than the buffer
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

    public void WriteBulkFrom(Stream source, int count)
    {
      Assert.ArgumentNotNull(() => source);
      Assert.ArgumentNonNegative(() => count);

      var bytesLeft = count;
      var chunkSize = 0;
      var bytesRead = 0;

      // absorb exceptions to maintain valid reader state
      // rethrow when we've properly read out all the bytes
      // TODO: This can probably be better functionally in a try/catch helper
      //       method that accepts the entire block below as a parameter
      // RedisReader.ReadBulkTo should looks about the same
      using (var wrapper = new AbsorbingStreamWrapper(_stream)) {
        while (bytesLeft > 0) {
          chunkSize = bytesLeft > _buffer.Length ? _buffer.Length : bytesLeft;
          bytesRead = source.Read(_buffer, 0, chunkSize);

          Assert.IsTrue(bytesRead > 0,
            () => new InvalidOperationException("Stream does not contains enough data."));

          wrapper.Write(_buffer, 0, bytesRead);
          bytesLeft -= bytesRead;
        }

        wrapper.ThrowIfError();
      }

      writeCrLf();
    }


    private void writeCrLf()
    {
      _stream.Write(CrLfBuffer, 0, CrLfBuffer.Length);
    }
  }
}
