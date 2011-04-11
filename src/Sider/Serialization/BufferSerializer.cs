
using System.IO;

namespace Sider
{
  public class BufferSerializer : SerializerBase<byte[]>
  {
    public override byte[] Read(Stream src, int length)
    {
      return ReadBytes(src, length);
    }


    public override int GetBytesNeeded(byte[] obj)
    {
      return obj.Length;
    }

    public override void Write(byte[] obj, Stream dest, int bytesNeeded)
    {
      dest.Write(obj, 0, obj.Length);
    }
  }
}
