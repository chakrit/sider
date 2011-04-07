
using System.IO;

namespace Sider
{
  public interface ITranslator<T>
  {
    T Read(RedisSettings settings, Stream src, int length);

    int GetBytesNeeded(RedisSettings settings);
    void Write(byte[] buffer, int offset, int count);

    void Reset();
  }
}
