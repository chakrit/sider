
using System.Collections.Generic;
using System.IO;

namespace Sider
{
  // TODO: Cleanup
  public partial class RedisClient<T>
  {
    private int readInt()
    { return readCore(ResponseType.Integer, r => r.ReadNumberLine()); }

    private long readInt64()
    { return readCore(ResponseType.Integer, r => r.ReadNumberLine64()); }

    private bool readBool()
    { return readCore(ResponseType.Integer, r => r.ReadNumberLine() == 1); }

    private bool readOk() { return readStatus("OK"); }

    private string readStatus()
    { return readCore(ResponseType.SingleLine, r => r.ReadStatusLine()); }

    private bool readStatus(string expectedMsg)
    {
      // TODO: Better way to detect status lines besides depending on
      // how redis explains the status?
      return readCore(ResponseType.SingleLine, r => r.ReadStatusLine() == expectedMsg);
    }

    private double readDouble()
    {
      return readCore(ResponseType.Bulk, r =>
      {
        var length = r.ReadNumberLine();
        return parseDouble(r.ReadBulk(length));
      });
    }

    private T readBulk()
    {
      return readCore(ResponseType.Bulk, r =>
      {
        var length = r.ReadNumberLine();
        return length < 0 ? default(T) : r.ReadSerializedBulk(_serializer, length);
      });
    }

    private string readStrBulk()
    {
      return readCore(ResponseType.Bulk, r =>
      {
        var length = r.ReadNumberLine();
        return length < 0 ? null : decodeStr(r.ReadBulk(length));
      });
    }

    private byte[] readBulkRaw()
    {
      return readCore(ResponseType.Bulk, r =>
      {
        var length = r.ReadNumberLine();
        return length < 0 ? null : r.ReadBulk(length);
      });
    }

    private int readBulkTo(Stream stream)
    {
      return readCore(ResponseType.Bulk, r =>
      {
        var length = r.ReadNumberLine();
        if (length > -1)
          r.ReadBulkTo(stream, length);

        return length;
      });
    }

    private T[] readMultiBulk()
    {
      return readCore(ResponseType.MultiBulk, r =>
      {
        var count = r.ReadNumberLine();
        if (count == -1)
          return null;

        var result = new T[count];

        for (var i = 0; i < count; i++) {
          var type = r.ReadTypeChar();
          SAssert.ResponseType(ResponseType.Bulk, type);

          var length = r.ReadNumberLine();
          if (length > -1)
            result[i] = r.ReadSerializedBulk(_serializer, length);
          else
            result[i] = default(T);
        }

        return result;
      });
    }

    private string[] readStrMultiBulk()
    {
      return readCore(ResponseType.MultiBulk, r =>
      {
        var count = r.ReadNumberLine();
        if (count == -1)
          return null;

        var result = new string[count];

        for (var i = 0; i < count; i++) {
          var type = r.ReadTypeChar();
          SAssert.ResponseType(ResponseType.Bulk, type);

          var length = r.ReadNumberLine();
          if (length > -1)
            result[i] = decodeStr(r.ReadBulk(length));
          else
            result[i] = null;
        }

        return result;
      });
    }

    private KeyValuePair<string, T>? readKeyValue()
    {
      return readCore(ResponseType.MultiBulk, r =>
      {
        var count = r.ReadNumberLine();
        if (count == -1)
          return (KeyValuePair<string, T>?)null;

        // read key
        var type = r.ReadTypeChar();
        SAssert.ResponseType(ResponseType.Bulk, type);

        var length = r.ReadNumberLine();
        var key = decodeStr(r.ReadBulk(length));

        // read value
        SAssert.ResponseType(ResponseType.Bulk, type = r.ReadTypeChar());

        length = r.ReadNumberLine();
        var value = r.ReadSerializedBulk(_serializer, length);

        return new KeyValuePair<string, T>(key, value);
      });
    }

    private KeyValuePair<string, T>[] readKeyValues()
    {
      return readCore(ResponseType.MultiBulk, r =>
      {
        var count = r.ReadNumberLine();
        if (count == -1)
          return null;

        var result = new KeyValuePair<string, T>[count];

        for (var i = 0; i < count; i++) {
          var type = r.ReadTypeChar();
          SAssert.ResponseType(ResponseType.Bulk, type);

          var length = r.ReadNumberLine();
          var key = decodeStr(r.ReadBulk(length));

          type = r.ReadTypeChar();
          SAssert.ResponseType(ResponseType.Bulk, type);

          length = r.ReadNumberLine();
          var value = r.ReadSerializedBulk(_serializer, length);

          result[i] = new KeyValuePair<string, T>(key, value);
        }

        return result;
      });
    }

    private KeyValuePair<string, string>[] readStringKeyValues()
    {
      return readCore(ResponseType.MultiBulk, r =>
      {
        var count = r.ReadNumberLine();
        if (count == -1)
          return null;

        var result = new KeyValuePair<string, string>[count];

        for (var i = 0; i < count; i++) {
          var type = r.ReadTypeChar();
          SAssert.ResponseType(ResponseType.Bulk, type);

          var length = r.ReadNumberLine();
          var key = decodeStr(r.ReadBulk(length));

          type = r.ReadTypeChar();
          SAssert.ResponseType(ResponseType.Bulk, type);

          length = r.ReadNumberLine();
          var value = decodeStr(r.ReadBulk(length));

          result[i] = new KeyValuePair<string, string>(key, value);
        }

        return result;
      });
    }
  }
}
