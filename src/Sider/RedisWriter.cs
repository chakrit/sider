
using System;
using System.Diagnostics;
using System.IO;

namespace Sider
{
  public class RedisWriter
  {
    public const int DefaultLineBufferSize = 256;


    private Stream _stream;
    private byte[] _buffer;

    public RedisWriter(Stream stream, int lineBufferSize = 256)
    {
      assert(stream != null, () => new ArgumentNullException("stream"));

      _stream = stream;
      _buffer = new byte[lineBufferSize];
    }


    [Conditional("DEBUG")]
    private void assert(bool condition,
      Func<Exception> exceptionIfFalse)
    {
      if (!condition)
        throw exceptionIfFalse();
    }
  }
}
