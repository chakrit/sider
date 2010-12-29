
using System.Collections.Generic;
using System.IO;

namespace Sider
{
  public partial class RedisClient
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

    private string readBulk()
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

    private string[] readMultiBulk()
    {
      return readCore(ResponseType.MultiBulk, r =>
      {
        var count = r.ReadNumberLine();
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

    private KeyValuePair<string, string>[] readKeyValues()
    {
      return readCore(ResponseType.MultiBulk, r =>
      {
        var count = r.ReadNumberLine();
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
