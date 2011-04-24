
using System.IO;

namespace Sider
{
  public interface ISerializer
  {
    void Init(RedisSettings settings);
  }

  // TODO: Change generic <T> to a capability test CanSerialize<T>() instead
  //   this way it maybe easier to write a serializer which can serialize
  //   multiple types such as object or EntityObject etc.
  public interface ISerializer<T> : ISerializer
  {
    T Read(Stream src, int length);

    int GetBytesNeeded(T obj);
    void Write(T obj, Stream dest, int bytesNeeded);
  }
}
