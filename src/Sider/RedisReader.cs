
using System;
using System.IO;
using System.Text;

namespace Sider
{
  public partial class RedisReader
  {
    private Stream _stream;

    public RedisReader(Stream stream)
    {
      Assert.ArgumentNotNull(() => stream);
      Assert.ArgumentSatisfy(() => stream, s => s.CanRead, "Stream must be readable.");

      _stream = stream;
    }


    public ResponseType ReadTypeChar()
    {
      var b = _stream.ReadByte();

      Assert.IsTrue(Enum.IsDefined(typeof(ResponseType), b),
        () => new ResponseException("Invalid response data, expected a response type character"));

      return (ResponseType)b;
    }

    public int ReadNumberLine()
    {
      bool negate = false;
      int num = 0;
      int b;

      // read the first byte which can be a minus '-' sign
      b = _stream.ReadByte();
      if (b == '-')
        negate = true;
      else
        num = b - '0';

      // read the rest of the numbers
      while ((b = _stream.ReadByte()) != -1) {
        if (b == '\r')
          break;

        // shifts the number by one digit, adding the current
        // digit to the end e.g. "99" + "7" = 99 * 10 + 7 = "997"
        num = num * 10 + b - '0';

        Assert.IsTrue(b >= '0' && b <= '9',
          () => new ResponseException("Expecting a digit, found instead: " + (char)b));
      }

      // consume leftover LF
      b = _stream.ReadByte();

      Assert.IsTrue(b == '\n',
        () => new ResponseException("Expecting CRLF, found instead: " + (char)b));

      return negate ? -num : num;
    }

    public string ReadStatusLine()
    {
      var sb = new StringBuilder();
      int b;

      while ((b = _stream.ReadByte()) != -1) {
        if (b == '\r') break;

        sb.Append((char)b);
      }

      // consume leftover LF
      b = _stream.ReadByte();

      Assert.IsTrue(b == '\n',
        () => new ResponseException("Expecting CRLF, found instead: " + (char)b));

      return sb.ToString();
    }

    public byte[] ReadBulk(int length)
    {
      Assert.ArgumentPositive(() => length);

      var buffer = new byte[length];
      ReadBulk(buffer, 0, length);

      return buffer;
    }

    public void ReadBulk(byte[] buffer, int offset, int bulkLength)
    {
      Assert.ArgumentNotNull(() => buffer);
      Assert.ArgumentPositive(() => bulkLength);
      Assert.ArgumentBetween(() => offset, 0, buffer.Length);

      // read data from the stream, expect as much data as there's bulkLength
      var bytesRead = _stream.Read(buffer, 0, bulkLength);

      Assert.IsTrue(bytesRead == bulkLength,
        () => new ResponseException("Expected " + bulkLength.ToString() +
          " bytes of bulk data, but only " + bytesRead.ToString() + " bytes are read."));

      // read out CRLF
      var b = _stream.ReadByte();
      b = _stream.ReadByte();

      Assert.IsTrue(b == '\n',
        () => new ResponseException("Expected CRLF, found instead: " + (char)b));
    }

    public void ReadBulk(Stream target, int bulkLength, int bufferSize = 256)
    {
      Assert.ArgumentNotNull(() => target);
      Assert.ArgumentPositive(() => bulkLength);
      Assert.ArgumentPositive(() => bufferSize);

      var buffer = new byte[bufferSize];
      var chunkSize = 0;
      var bytesLeft = bulkLength;
      var bytesRead = 0;

      while (bytesLeft > 0) {
        chunkSize = bytesLeft > buffer.Length ? buffer.Length : bytesLeft;
        bytesLeft -= (bytesRead = _stream.Read(buffer, 0, chunkSize));

        target.Write(buffer, 0, bytesRead);
      }

      // eat up crlf
      var b = _stream.ReadByte();
      b = _stream.ReadByte();

      Assert.IsTrue(b == '\n',
        () => new ResponseException("Expected CRLF, got '" + ((char)b) + "' instead."));
    }
  }
}
