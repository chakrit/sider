
using System.IO;

namespace Sider
{
  public class StringTranslator : ITranslator<string>
  {
    public string Read(RedisSettings settings, Stream src, int length)
    {
      throw new System.NotImplementedException();
    }

    public int GetBytesNeeded(RedisSettings settings)
    {
      throw new System.NotImplementedException();
    }

    public void Write(byte[] buffer, int offset, int count)
    {
      throw new System.NotImplementedException();
    }

    public void Reset()
    {
      throw new System.NotImplementedException();
    }
  }
}
