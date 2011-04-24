
using System;
using System.Collections.Generic;
using System.IO;

namespace Sider
{
  // A reader that's a bit more aware of the redis protocol than RedisReader
  public class ProtocolReader : SettingsWrapper
  {
    private Stream _stream;
    private RedisReader _reader;
    private ProtocolEncoder _encoder;

    public ProtocolReader(RedisSettings settings, ProtocolEncoder encoder,
      Stream stream) :
      base(settings)
    {
      _stream = stream;
      _reader = new RedisReader(stream, settings);
      _encoder = encoder;
    }


    public int ReadInt()
    {
      readType(ResponseType.Integer);
      return _reader.ReadNumberLine();
    }

    public long ReadInt64()
    {
      readType(ResponseType.Integer);
      return _reader.ReadNumberLine64();
    }

    public double ReadDouble()
    {
      return _encoder.DecodeDouble(readBulk());
    }

    public bool ReadBool()
    {
      readType(ResponseType.Integer);
      return _reader.ReadNumberLine() == 1;
    }

    public DateTime ReadDateTime()
    {
      readType(ResponseType.Integer);
      return _encoder.DecodeDateTime(_reader.ReadNumberLine64());
    }

    public TimeSpan ReadTimeSpan()
    {
      readType(ResponseType.Integer);
      return _encoder.DecodeTimeSpan(_reader.ReadNumberLine64());
    }

    public string ReadStatus()
    {
      readType(ResponseType.SingleLine);
      return _reader.ReadStatusLine();
    }

    public bool ReadStatus(string expectedMessage)
    {
      return ReadStatus() == expectedMessage;
    }

    public bool ReadOk() { return ReadStatus("OK"); }
    public bool ReadQueued() { return ReadStatus("QUEUED"); }


    public string ReadStrBulk()
    {
      return _encoder.DecodeStr(readBulk());
    }

    public byte[] ReadRawBulk()
    {
      readType(ResponseType.Bulk);

      var length = _reader.ReadNumberLine();
      return length < 0 ? null : _reader.ReadBulk(length);
    }

    public int ReadStreamedBulk(Stream stream)
    {
      var length = _reader.ReadNumberLine();
      if (length > -1)
        _reader.ReadBulkTo(stream, length);

      return length;
    }

    public T ReadSerializedBulk<T>(ISerializer<T> serializer)
    {
      readType(ResponseType.Bulk);
      var length = _reader.ReadNumberLine();
      return length < 0 ?
        default(T) :
        _reader.ReadSerializedBulk(serializer, length);
    }

    public string[] ReadStrMultiBulk()
    {
      readType(ResponseType.MultiBulk);

      var count = _reader.ReadNumberLine();
      if (count == -1)
        return null;

      var result = new string[count];
      for (var i = 0; i < result.Length; i++)
        result[i] = ReadStrBulk();

      return result;
    }

    public T[] ReadSerializedMultiBulk<T>(ISerializer<T> serializer)
    {
      readType(ResponseType.MultiBulk);

      var count = _reader.ReadNumberLine();
      if (count == -1)
        return null;

      var result = new T[count];
      for (var i = 0; i < result.Length; i++)
        result[i] = ReadSerializedBulk<T>(serializer);

      return result;
    }


    public KeyValuePair<string, string>? ReadStrKeyValue()
    {
      var arr = ReadStrMultiBulk();
      return arr == null ?
        (KeyValuePair<string, string>?)null :
        new KeyValuePair<string, string>(arr[0], arr[1]);
    }

    public KeyValuePair<string, T>? ReadSerializedKeyValue<T>(ISerializer<T> serializer)
    {
      readType(ResponseType.MultiBulk);

      var count = _reader.ReadNumberLine();
      if (count == -1)
        return null;

      return new KeyValuePair<string, T>(
        ReadStrBulk(),
        ReadSerializedBulk<T>(serializer));
    }

    public KeyValuePair<string, string>[] ReadStrKeyValues()
    {
      readType(ResponseType.MultiBulk);

      var count = _reader.ReadNumberLine();
      if (count == -1)
        return null;

      var result = new KeyValuePair<string, string>[count / 2];
      for (var i = 0; i < result.Length; i++) {
        result[i] = new KeyValuePair<string, string>(
          ReadStrBulk(),
          ReadStrBulk());
      }

      return result;
    }

    public KeyValuePair<string, T>[] ReadSerializedKeyValues<T>(
      ISerializer<T> serializer)
    {
      readType(ResponseType.MultiBulk);

      var count = _reader.ReadNumberLine();
      if (count == -1)
        return null;

      var result = new KeyValuePair<string, T>[count / 2];
      for (var i = 0; i < result.Length; i++) {
        result[i] = new KeyValuePair<string, T>(
          ReadStrBulk(),
          ReadSerializedBulk<T>(serializer));
      }

      return result;
    }


    private void readType(ResponseType expectedType)
    {
      var type = _reader.ReadTypeChar();
      SAssert.ResponseType(expectedType, type, _reader);
    }

    private ArraySegment<byte> readBulk()
    {
      readType(ResponseType.Bulk);
      var length = _reader.ReadNumberLine();

      // if fit in encoder buffer, use that
      if (length <= _encoder.SharedBuffer.Length) {
        _reader.ReadBulk(_encoder.SharedBuffer, 0, length);
        return new ArraySegment<byte>(_encoder.SharedBuffer, 0, length);
      }

      return new ArraySegment<byte>(_reader.ReadBulk(length));
    }
  }
}
