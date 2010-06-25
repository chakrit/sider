
using System;
using System.IO;
using System.Text;

namespace Sider
{
  internal partial class RedisReader
  {
    public const int DefaultBufferSize = 4 * 1024;
    public const int DefaultLineBufferSize = 1024;


    private Stream _stream;

    private RingBuffer _buffer;
    private int _offset;
    private int _maxOffset;

    private int _lineBufferSize;


    public RedisReader(Stream s,
      int bufferSize = DefaultBufferSize,
      int lineBufferSize = DefaultLineBufferSize)
    {
      _stream = s;

      _buffer = new RingBuffer(bufferSize);
      _offset = 0;
      _maxOffset = -1;

      _lineBufferSize = lineBufferSize;
    }


    private void readTypeChar(Action<Action> fillFunc,
      Action<ResponseType> callback)
    {
      fillFunc(() => callback((ResponseType)_buffer[_offset++]));
    }

    private void readSingleLine(Action<Action> fillFunc,
      Action<string> callback)
    {
      // TODO: Eliminate this need for line buffer
      var lineBuffer = new byte[_lineBufferSize];
      var lineOffset = 0;

      var startOffset = _offset;

      // scans until we ate a CRLF
      scanUntil(fillFunc, () =>
        _offset > 1 &&
        _buffer[_offset - 2] == 0x0D &&
        _buffer[_offset - 1] == 0x0A,

        // collecting bytes into the line buffer in the meanwhile
        b => lineBuffer[lineOffset++] = b,

        // when we're done, spits out CRLF and parse the line buffer as a string
        () =>
        {
          lineOffset -= 2;
          callback(Encoding.Default.GetString(lineBuffer, 0, lineOffset));
        });
    }

    // scan untils the predicate is true, automatically filling up buffers as needed
    // and call the callback once the predicate is true
    private void scanUntil(Action<Action> fillFunc,
      Func<bool> predicate,
      Action<byte> bytesCollector,
      Action callback)
    {
      fillFunc(() =>
      {
        if (predicate()) return;

        // scan until the predicate is false, or we've reached the end of the buffer
        while (!(predicate() || _offset > _maxOffset))
          bytesCollector(_buffer[_offset++]);

        // predicate is false, we're done
        if (predicate()) {
          callback();
          return;
        }

        // if we've reached the end of the buffer but the predicate
        // is still true, fill another buffer and continue scanning
        scanUntil(fillFunc, predicate, bytesCollector, callback);
      });
    }


    private void fillBuffer(Action callback)
    {
      fillBufferCore(callback_ =>
        callback_(_buffer.ReadFrom(_stream, _maxOffset + 1, _buffer.Size)),
        callback);
    }

    private void asyncFillBuffer(Action callback)
    {
      fillBufferCore(callback_ =>
      {
        _buffer.BeginReadFrom(_stream, _maxOffset + 1, _buffer.Size, ar =>
          callback_(_buffer.EndReadFrom(ar)), null);
      }, callback);
    }

    private void fillBufferCore(Action<Action<int>> fillAct, Action callback)
    {
      if (_offset > _maxOffset)
        fillAct(bytesRead =>
        {
          _maxOffset += bytesRead;
          callback();
        });
      else
        callback();
    }
  }
}
