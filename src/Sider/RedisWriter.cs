
using System.IO;
using System.Text;

namespace Sider
{
  internal class RedisWriter
  {
    public const int DefaultLineBufferSize = 1024;


    private Stream _stream;
    private byte[] _buffer;
    private byte[] _crLf;

    public RedisWriter(Stream s, int lineBufferSize = DefaultLineBufferSize)
    {
      _stream = s;
      _buffer = new byte[DefaultLineBufferSize];

      _crLf = new byte[] { 0x0D, 0x0A };
    }


    public void WriteLine()
    {
      _stream.Write(_crLf, 0, _crLf.Length);
      _stream.Flush();
    }

    public void WriteLine(string line)
    {
      var length = encode(line, _buffer);

      _stream.Write(_buffer, 0, length);
      _stream.Write(_crLf, 0, _crLf.Length);
      _stream.Flush();
    }

    public void Write(string text)
    {
      var length = encode(text, _buffer);

      _stream.Write(_buffer, 0, length);
      _stream.Flush();
    }

    public void Write(byte[] bytes)
    {
      _stream.Write(bytes, 0, bytes.Length);
      _stream.Flush();
    }

    public void Write(byte[] bytes, int offset, int count)
    {
      _stream.Write(bytes, offset, count);
      _stream.Flush();
    }


    private int encode(string s, byte[] buffer)
    {
      return Encoding.Default.GetBytes(s, 0, s.Length, buffer, 0);
    }
  }
}
