
using System;
using System.IO;
using System.Text;

namespace Sider
{
  public class StringTranslator : TranslatorBase<string>
  {
    private Encoding _encoding;
    private int _bytesNeeded;

    private byte[] _tempBuffer;
    private int _tempOffset;
    private int _bytesLeft;

    public StringTranslator() : this(Encoding.UTF8) { }

    public StringTranslator(Encoding enc) { _encoding = enc; }


    public override string Read(RedisSettings settings, Stream src, int length)
    {
      return _encoding.GetString(ReadBytes(src, length));
    }


    public int GetBytesNeeded(RedisSettings settings)
    {
      return _bytesNeeded = _encoding.GetByteCount(Object);
    }

    public int Write(byte[] buffer, int offset, int count)
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
