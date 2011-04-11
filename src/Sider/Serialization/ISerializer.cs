
using System.IO;

namespace Sider
{
  public interface ISerializer
  {
    RedisSettings Settings { get; }
  }

  public interface ISerializer<T> : ISerializer
  {
    T Read(Stream src, int length);

    int GetBytesNeeded(T obj);
    void Write(T obj, Stream dest, int bytesNeeded);
  }
}
