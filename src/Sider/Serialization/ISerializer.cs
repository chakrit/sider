
using System.IO;

namespace Sider
{
  public interface ISerializer<T>
  {
    T Read(RedisSettings settings, Stream src, int length);

    void ResetWrite(T obj);
    void Cleanup();

    int GetBytesNeeded(RedisSettings settings);
    int Write(byte[] buffer, int offset, int count);
  }
}
