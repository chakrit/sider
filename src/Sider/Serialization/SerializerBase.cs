
using System;
using System.IO;

namespace Sider
{
  public abstract class SerializerBase<T> : ISerializer<T>
  {
    public RedisSettings Settings { get; private set; }

    public SerializerBase() : this(null) { /* doesn't use settings */ }
    public SerializerBase(RedisSettings settings) { Settings = settings; }


    public void Init(RedisSettings settings)
    {
      Settings = settings;
      OnInit();
    }

    protected virtual void OnInit() { }


    public abstract T Read(Stream src, int length);

    public abstract int GetBytesNeeded(T obj);
    public abstract void Write(T obj, Stream dest, int bytesNeeded);


    protected byte[] ReadBytes(Stream src, int length)
    {
      var buffer = new byte[length];
      var bytesLeft = length;

      while (bytesLeft > 0)
        bytesLeft -= src.Read(buffer, length - bytesLeft, bytesLeft);

      return buffer;
    }

    protected void StreamCopy(Stream src, Stream dest, int count,
      int bufferSize = 4096)
    {
      var bytesLeft = count;
      var buffer = new byte[bufferSize];
      while (bytesLeft > 0) {
        var chunkSize = Math.Min(bytesLeft, buffer.Length);
        var bytesRead = src.Read(buffer, 0, chunkSize);

        dest.Write(buffer, 0, bytesRead);
        bytesLeft -= bytesRead;
      }
    }
  }
}
