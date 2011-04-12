
namespace Sider.Serialization
{
  // TODO: Make public + user configurable
  internal static class Serializers
  {
    public static ISerializer<T> For<T>()
    {
      if (typeof(T) == typeof(string))
        return (ISerializer<T>)new StringSerializer();

      if (typeof(T) == typeof(byte[]))
        return (ISerializer<T>)new BufferSerializer();

      return (ISerializer<T>)new ObjectSerializer<T>();
    }
  }
}
