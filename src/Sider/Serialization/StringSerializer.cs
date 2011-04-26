
using System.IO;
using System.Text;

namespace Sider.Serialization
{
  public class StringSerializer : SerializerBase<string>
  {
    private Encoding _encoding;
    private byte[] _buffer;

    protected override void OnInit()
    {
      _encoding = Settings.ValueEncoding;
      _buffer = new byte[Settings.SerializationBufferSize];
    }


    public override string Read(Stream src, int length)
    {
      return _encoding.GetString(ReadBytes(src, length));
    }


    public override int GetBytesNeeded(string obj)
    {
      return _encoding.GetByteCount(obj);
    }

    public override void Write(string obj, Stream dest, int bytesNeeded)
    {
      if (bytesNeeded > _buffer.Length) {
        // allocate a new temporary buffer, if the buffer is not large enough
        var buffer = _encoding.GetBytes(obj);
        dest.Write(buffer, 0, buffer.Length);
      }
      else {
        var bytesTotal = _encoding.GetBytes(obj, 0, obj.Length, _buffer, 0);
        dest.Write(_buffer, 0, bytesTotal);
      }
    }
  }
}
