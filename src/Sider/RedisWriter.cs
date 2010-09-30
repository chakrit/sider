
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

    public RedisWriter(Stream stream, int bufferSize = DefaultBufferSize)
    {
      SAssert.ArgumentNotNull(() => stream);
      SAssert.ArgumentPositive(() => bufferSize);
      SAssert.ArgumentSatisfy(() => stream, s => s.CanWrite, "Stream must be writable.");

      _stream = stream;
      _buffer = new byte[bufferSize];
    }


    public void WriteLine(string str)
    {
      SAssert.ArgumentNotNull(() => str);

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
      SAssert.ArgumentSatisfy(() => type,
        v => Enum.IsDefined(typeof(ResponseType), v), "Invalid type char.");

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

      _stream.Write(buffer, offset, count);
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
          chunkSize = bytesLeft > _buffer.Length ? _buffer.Length : bytesLeft;
          bytesLeft -= bytesRead = wrapper.Read(_buffer, 0, chunkSize);

          SAssert.IsTrue(bytesRead > 0,
            () => new InvalidOperationException("Stream does not contains enough data."));

          _stream.Write(_buffer, 0, bytesRead);
        }

        writeCrLf();
        wrapper.ThrowIfError();
      }
    }


    private void writeCrLf()
    {
      _stream.Write(CrLfBuffer, 0, CrLfBuffer.Length);
    }
  }
}
