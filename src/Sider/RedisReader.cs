
using System;
using System.IO;
using System.Text;

namespace Sider
{
  // see redis protocol specification for more info
  // http://code.google.com/p/redis/wiki/ProtocolSpecification
  public partial class RedisReader
  {
    private Stream _stream;
    private RedisSettings _settings;


    public RedisReader(Stream stream) : this(stream, new RedisSettings()) { }

    public RedisReader(Stream stream, RedisSettings settings)
    {
      SAssert.ArgumentNotNull(() => stream);
      SAssert.ArgumentSatisfy(() => stream, s => s.CanRead, "Stream must be readable.");
      SAssert.ArgumentNotNull(() => settings);

      _settings = settings;
      _stream = new BufferedStream(stream, _settings.ReadBufferSize);
    }


    public ResponseType ReadTypeChar()
    {
      var b = _stream.ReadByte();

      SAssert.IsTrue(Enum.IsDefined(typeof(ResponseType), b),
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
        num = (num * 10) + b - '0';

        SAssert.IsTrue(b >= '0' && b <= '9',
          () => new ResponseException("Expecting a digit, found instead: " + (char)b));
      }

      // consume leftover LF
      b = _stream.ReadByte();

      SAssert.IsTrue(b == '\n',
        () => new ResponseException("Expecting CRLF, found instead: " + (char)b));

      return negate ? -num : num;
    }

    // same implementation as ReadNumberLine() but expecting 64-bit values
    public long ReadNumberLine64()
    {
      bool negate = false;
      long num = 0;
      int b;

      b = _stream.ReadByte();
      if (b == '-')
        negate = true;
      else
        num = b - '0';

      while ((b = _stream.ReadByte()) != -1) {
        if (b == '\r') break;

        num = (num * 10) + b - '0';

        SAssert.IsTrue(b >= '0' && b <= '9',
          () => new ResponseException("Expected a digit, found instead: " + (char)b));
      }

      b = _stream.ReadByte();

      SAssert.IsTrue(b == '\n',
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

      SAssert.IsTrue(b == '\n',
        () => new ResponseException("Expecting CRLF, found instead: " + (char)b));

      return sb.ToString();
    }

    public byte[] ReadBulk(int length)
    {
      SAssert.ArgumentNonNegative(() => length);

      var buffer = new byte[length];
      ReadBulk(buffer, 0, length);

      return buffer;
    }

    public void ReadBulk(byte[] buffer, int offset, int bulkLength)
    {
      SAssert.ArgumentNotNull(() => buffer);
      SAssert.ArgumentNonNegative(() => bulkLength);

      // special case for empty reads
      if (offset == 0 && bulkLength == 0) return;

      SAssert.ArgumentBetween(() => offset, 0, buffer.Length);
      SAssert.ArgumentSatisfy(() => offset, o => o + bulkLength <= buffer.Length,
        "Offset plus bulkLength is larger than the supplied buffer.");

      // read data from the stream, expect as much data as there's bulkLength
      var bytesRead = _stream.Read(buffer, 0, bulkLength);

      SAssert.IsTrue(bytesRead == bulkLength,
        () => new ResponseException("Expected " + bulkLength.ToString() +
          " bytes of bulk data, but only " + bytesRead.ToString() + " bytes are read."));

      // read out CRLF
      var b = _stream.ReadByte();
      b = _stream.ReadByte();

      SAssert.IsTrue(b == '\n',
        () => new ResponseException("Expecting CRLF, found instead: " + (char)b));
    }

    public void ReadBulkTo(Stream target, int bulkLength)
    {
      ReadBulkTo(target, bulkLength, _settings.ReadBufferSize);
    }

    public void ReadBulkTo(Stream target, int bulkLength, int bufferSize)
    {
      SAssert.ArgumentNotNull(() => target);
      SAssert.ArgumentNonNegative(() => bulkLength);
      SAssert.ArgumentPositive(() => bufferSize);

      using (var limiter = new LimitingStream(_stream, bulkLength))
        limiter.CopyTo(target);

      // eat up crlf
      var b = _stream.ReadByte();
      b = _stream.ReadByte();

      SAssert.IsTrue(b == '\n',
        () => new ResponseException("Expected CRLF, got '" + ((char)b) + "' instead."));
    }

    public T ReadSerializedBulk<T>(ISerializer<T> serializer, int bulkLength)
    {
      SAssert.ArgumentNotNull(() => serializer);
      SAssert.ArgumentNonNegative(() => bulkLength);

      return serializer.Read(_stream, bulkLength);
    }
  }
}

