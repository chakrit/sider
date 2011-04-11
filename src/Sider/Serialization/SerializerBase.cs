
using System;
using System.IO;

namespace Sider
{
  public abstract class SerializerBase<T> : ISerializer<T>
  {
    protected T Object { get; private set; }


    public abstract T Read(RedisSettings settings, Stream src, int length);


    public virtual void ResetWrite(T obj) { Object = obj; }
    public virtual void Cleanup() { Object = default(T); /* null */ }

    public abstract int GetBytesNeeded(RedisSettings settings);
    public abstract int Write(byte[] buffer, int offset, int count);


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
