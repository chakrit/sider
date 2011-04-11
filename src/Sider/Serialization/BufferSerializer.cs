
namespace Sider
{
  public class BufferSerializer : SerializerBase<byte[]>
  {
    public override byte[] Read(RedisSettings settings, System.IO.Stream src, int length)
    {
      throw new System.NotImplementedException();
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
