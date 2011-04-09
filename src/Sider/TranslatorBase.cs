
using System.IO;

namespace Sider
{
  public abstract class TranslatorBase<T> : ITranslator<T>
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
  }
}
