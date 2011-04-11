
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Sider.Serialization
{
  public class ObjectSerializer : SerializerBase<object>
  {
    private BinaryFormatter _formatter = new BinaryFormatter();
    private MemoryStream _mem = new MemoryStream();


    public override object Read(RedisSettings settings, Stream src, int length)
    {
      return _formatter.Deserialize(new LimitingStream(src, length));
    }


    public override int GetBytesNeeded(RedisSettings settings)
    {
      // TODO: Could switch on a type-specific serializer which might be faster
      _mem.SetLength(0);
      _formatter.Serialize(_mem, Object);

      _mem.Seek(0, SeekOrigin.Begin);
      return (int)_mem.Length;
    }

    public override int Write(byte[] buffer, int offset, int count)
    {
      return _mem.Read(buffer, offset, count);
    }
  }
}
