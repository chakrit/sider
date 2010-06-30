
using System;
using System.Text;

namespace Sider
{
  public partial class RedisClient
  {
    private static byte[] encodeStr(string s) { return Encoding.UTF8.GetBytes(s); }
    private static string decodeStr(byte[] raw) { return Encoding.UTF8.GetString(raw); }

    private static string formatDouble(double d)
    {
      return double.IsPositiveInfinity(d) ? "+inf" :
        double.IsNegativeInfinity(d) ? "-inf" :
        d.ToString("0.0");
    }

    private static double parseDouble(byte[] raw)
    {
      var str = Encoding.Default.GetString(raw);

      return str == "inf" || str == "+inf" ? double.PositiveInfinity :
        str == "-inf" ? double.NegativeInfinity :
        double.Parse(str);
    }


    private void writeCmd(string command, string key)
    {
      writeCore(w => w.WriteLine("{0} {1}".F(command, key)));
    }

    private void writeCmd(string command, string[] keys)
    {
      writeCore(w => w.WriteLine("{0} {1}".F(command, string.Join(" ", keys))));
    }

    private void writeCmd(string command, string key, string value)
    {
      writeCmd(command, key, encodeStr(value));
    }

    private void writeCmd(string command, string key, byte[] data)
    {
      writeCore(w =>
      {
        w.WriteLine("{0} {1} {2}".F(command, key, data.Length));
        w.WriteBulk(data);
      });
    }

    private void writeListCmd(string command, string key, int min, int max)
    {
      writeCore(w => w.WriteLine("{0} {1} {2} {3}".F(
        command, key, min, max)));
    }

    private void writeZSetCmd(string command, string key, double score, string value)
    {
      writeCore(w =>
      {
        var raw = encodeStr(value);

        w.WriteLine("{0} {1} {2} {3}".F(command, key, formatDouble(score), raw.Length));
        w.WriteBulk(raw);
      });
    }

    private void writeZSetCmd(string command, string key, double min, double max)
    {
      writeCore(w => w.WriteLine("{0} {1} {2} {3}".F(
        command, key, formatDouble(min), formatDouble(max))));
    }


    private int readInt()
    { return readCore(ResponseType.Integer, r => r.ReadNumberLine()); }

    private long readInt64()
    { return readCore(ResponseType.Integer, r => r.ReadNumberLine64()); }

    private bool readBool()
    { return readCore(ResponseType.Integer, r => r.ReadNumberLine() == 1); }

    private bool readStatus(string expectedMsg)
    {
      return readCore(ResponseType.SingleLine, r => r.ReadStatusLine() == expectedMsg);
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

    private string[] readMultiBulk()
    {
      return readCore(ResponseType.MultiBulk, r =>
      {
        var count = r.ReadNumberLine();
        var result = new string[count];

        for (var i = 0; i < count; i++) {
          var type = _reader.ReadTypeChar();
          Assert.ResponseType(ResponseType.Bulk, type);

          var length = _reader.ReadNumberLine();
          if (length > -1)
            result[i] = decodeStr(_reader.ReadBulk(length));
          else
            result[i] = null;
        }

        return result;
      });
    }


    private void writeCore(Action<RedisWriter> writeAction)
    {
      // TODO: Add pipelining support by recording writes
      // TODO: Add logging
      writeAction(_writer);
    }

    private T readCore<T>(ResponseType expectedType, Func<RedisReader, T> readFunc)
    {
      // TODO: Add pipelining support by recording reads
      // TODO: Add logging
      // TODO: Add error-checking support to reads
      var type = _reader.ReadTypeChar();
      Assert.ResponseType(expectedType, type);

      return readFunc(_reader);
    }
  }
}
