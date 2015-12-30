using System;
using System.IO;
using System.Text;

namespace Sider {
  public class RedisReader : IDisposable {
    readonly Stream stream;

    public RedisReader(Stream stream) {
      if (stream == null) throw new ArgumentNullException("stream");

      this.stream = stream;
    }

    public void Dispose() {
      stream.Dispose();
    }

    public ResponseType ReadType() {
      var b = stream.ReadByte();
      if (!Enum.IsDefined(typeof(ResponseType), b)) {
        throw new ProtocolException("type specifier", new string(new char[] { (char)b }));
      }

      return (ResponseType)b;
    }

    public void ReadCRLF() {
      var cr = stream.ReadByte();
      var lf = stream.ReadByte();

      if (cr != '\r' || lf != '\n') {
        throw new ProtocolException("\\r\\n", (char)cr, (char)lf);
      }
    }

    public string ReadStatusLine() {
      var buffer = ReadRawLine();
      return Encoding.UTF8.GetString(buffer, 0, buffer.Length);
    }

    public long ReadNumberLine() {
      var buffer = ReadRawLine();
      if (buffer.Length == 0) return 0;

      int i = 0;
      var negate = false;
      if (buffer[0] == '-') {
        negate = true;
        i += 1;
      } else if (buffer[0] == '+') {
        i += 1;
      }

      long result = 0;
      for (; i < buffer.Length; i++) {
        var b = buffer[i];
        if (!('0' <= b && b <= '9')) {
          throw new ProtocolException("a digit", (char)b);
        }

        result = result * 10 + (b - '0');
      }

      return negate ? -result : result;
    }

    public int ReadBulk(byte[] buffer, int offset, int length) {
      if (buffer == null) throw new ArgumentNullException("buffer");
      if (offset < 0) throw new ArgumentException("offset cannot be negative");
      if (length < 0) throw new ArgumentException("length cannot be negative");
      if ((offset + length) > buffer.Length) {
        throw new ArgumentException("offset+length is beyond buffer bounds");
      }

      var remaining = length;
      while (remaining > 0) {
        var read = stream.Read(buffer, offset, remaining);
        if (read == -1) throw new EndOfStreamException("expected to read " + remaining.ToString() + " more bytes");

        remaining -= read;
        offset += read;
      }

      return length;
    }

    byte[] ReadRawLine() {
      int b;
      byte[] buffer;

      using (var ms = new MemoryStream(64)) {
        while ((b = stream.ReadByte()) != -1) {
          if (b == '\r') break;
          ms.WriteByte((byte)b);
        }

        buffer = ms.ToArray();
      }

      int lf = stream.ReadByte();
      if (lf != '\n') {
        throw new ProtocolException("\\r\\n", (char)b, (char)lf);
      }

      return buffer;
    }
  }
}

