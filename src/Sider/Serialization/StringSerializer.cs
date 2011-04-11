
using System;
using System.IO;
using System.Text;

namespace Sider
{
  public class StringSerializer : SerializerBase<string>
  {
    private Encoding _encoding;
    private int _bytesNeeded;

    private byte[] _tempBuffer;
    private int _tempOffset;
    private int _bytesLeft;

    public StringSerializer() : this(Encoding.UTF8) { }

    public StringSerializer(Encoding enc) { _encoding = enc; }


    public override string Read(RedisSettings settings, Stream src, int length)
    {
      return _encoding.GetString(ReadBytes(src, length));
    }


    public override int GetBytesNeeded(RedisSettings settings)
    {
      return _bytesNeeded = _encoding.GetByteCount(Object);
    }

    public override int Write(byte[] buffer, int offset, int count)
    {
      if (_tempBuffer == null) {
        if (_bytesNeeded <= count)
          return _encoding.GetBytes(Object, 0, Object.Length, buffer, offset);

        // _bytesNeeded > count --> needs to allocate a tempoary buffer
        _tempBuffer = new byte[_bytesNeeded];
        _encoding.GetBytes(Object, 0, Object.Length, _tempBuffer, 0);
        _bytesLeft = _bytesNeeded;
      }

      // in _tempBuffer mode
      var chunkSize = Math.Min(count, _bytesLeft);
      Buffer.BlockCopy(_tempBuffer, _bytesNeeded - _bytesLeft,
        buffer, offset, chunkSize);

      _bytesLeft -= chunkSize;
      return chunkSize;
    }
  }
}
