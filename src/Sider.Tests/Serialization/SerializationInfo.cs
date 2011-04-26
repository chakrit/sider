
using System.IO;
using Sider.Serialization;

namespace Sider.Tests.Serialization
{
  public class SerializationInfo<T> where T : ISerializer
  {
    public T Serializer { get; set; }
    public object Object { get; set; }

    public Stream TempStream { get; set; }
    public byte[] TempBuffer { get; set; }

    public RedisSettings Settings { get; set; }

    public SerializationInfo(T serializer)
    {
      Serializer = serializer;
      Object = new TestObj("Hello", 2, "World", 3);

      TempStream = new MemoryStream();
      TempBuffer = new byte[4096];

      Settings = RedisSettings.Default;
    }


    public void RewindStream()
    {
      TempStream.Seek(0, SeekOrigin.Begin);
    }

  }
}
