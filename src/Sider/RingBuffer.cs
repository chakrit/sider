
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace Sider
{
  [DebuggerStepThrough]
  internal class RingBuffer
  {
    private delegate void ReadCoreDelegate(int offset, int count);


    public const int DefaultBufferSize = 4096;


    private byte[] _buffer;
    private int _maxOffset;


    public int Size { get { return _buffer.Length; } }

    public byte this[int offset]
    {
      get { return _buffer[offset % _buffer.Length]; }
      set { _buffer[offset % _buffer.Length] = value; }
    }


    public RingBuffer(int bufferSize = DefaultBufferSize)
    {
      _buffer = new byte[bufferSize];
    }

    public RingBuffer(byte[] buffer)
    {
      _buffer = buffer;
    }


    public int ReadFrom(Stream s, int offset, int count)
    {
      var result = default(int);

      readCore(offset, count, (offset_, count_) =>
        result = s.Read(_buffer, offset_, count_));

      return result;
    }

    public IAsyncResult BeginReadFrom(Stream s, int offset, int count,
      AsyncCallback callback, object asyncState)
    {
      var result = new SiderAsyncResult<int>(asyncState);

      readCore(offset, count, (offset_, count_) =>
        s.BeginRead(_buffer, offset_, count_,
          result.MakeAsyncCallback(s.EndRead, callback), null));

      return result;
    }

    public int EndReadFrom(IAsyncResult ar)
    {
      return ((SiderAsyncResult<int>)ar).Result;
    }


    private void readCore(int offset, int count,
      ReadCoreDelegate reader)
    {
      if (count > _buffer.Length)
        throw new ArgumentException(
          "Read count cannot be larger than RingBuffer.Size.", "count");
      if (offset < 0)
        throw new ArgumentOutOfRangeException(
          "Read offset cannot be less than zero.", "offset");


      var startOffset = offset % _buffer.Length;
      var endOffset = startOffset + count;

      // read normally if we're still within bounds
      // else read just enough to fill up current buffer
      // so the next read starts at the beginning (max % size) == 0
      count = (endOffset <= _buffer.Length) ?
        count :
        _buffer.Length - startOffset;

      reader(startOffset, count);
    }
  }
}