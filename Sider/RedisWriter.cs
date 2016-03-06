using System.IO;
using System;
using System.Text;

namespace Sider {
  public class RedisWriter : IDisposable {
    readonly Stream stream;
    readonly RedisSettings settings;

    public RedisWriter(Stream stream, RedisSettings settings) {
      if (stream == null) throw new ArgumentNullException("stream");
      if (settings == null) throw new ArgumentNullException("settings");

      this.stream = stream;
      this.settings = settings;
    }

    public void Dispose() {
      stream.Dispose();
    }

    public void WriteType(ResponseType type) {
      if (!Enum.IsDefined(typeof(ResponseType), type)) {
        throw new ProtocolException("type specified", new string(new[] { (char)type }));
      }

      stream.WriteByte((byte)type);
    }

    public void WriteStringLine(string line) {
      if (string.IsNullOrEmpty(line)) throw new ArgumentNullException("line");

      // TODO: Re-use encoding buffer, or use a dedeciated encoder/decoder class.
      var buffer = Encoding.UTF8.GetBytes(line + "\r\n");
      stream.Write(buffer, 0, buffer.Length);
    }
  }
}

