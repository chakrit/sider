
using System.IO;

namespace Sider
{
  public class BufferTranslator : TranslatorBase<byte[]>
  {
    public override byte[] Read(RedisSettings settings, Stream src, int length)
    {

    }

    public override int GetBytesNeeded(RedisSettings settings)
    {
      throw new System.NotImplementedException();
    }

    public override int Write(byte[] buffer, int offset, int count)
    {
      throw new System.NotImplementedException();
    }
  }
}
