
using System.IO;

namespace Sider
{
  public interface ISerializer
  {
  }

  public interface ISerializer<T> : ISerializer
  {
    RedisSettings Settings { get; }

    T Read(Stream src, int length);

    int GetBytesNeeded(T obj);
    void Write(T obj, Stream dest, int bytesNeeded);
  }
}
