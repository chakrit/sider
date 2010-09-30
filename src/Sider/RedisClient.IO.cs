
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Sider
{
  // TODO: Detect invalid client state and self-dispose or restart
  //       e.g. when protocol errors occour
  public partial class RedisClient
  {
    // 1st Jan 1970
    public const long UnixEpochL = 621355968000000000L;
    public static readonly DateTime UnixEpoch = new DateTime(UnixEpochL);


    // TODO: use a shared/reusable buffer?
    private static byte[] encodeStr(string s) { return Encoding.UTF8.GetBytes(s); }
    private static string decodeStr(byte[] raw) { return Encoding.UTF8.GetString(raw); }

    private static string formatDateTime(DateTime dt)
    {
      return (dt - UnixEpoch).TotalSeconds.ToString();
    }
    private static DateTime parseDateTime(long dateValue)
    {
      return new DateTime(dateValue + UnixEpochL);
    }

    private static string formatTimeSpan(TimeSpan t)
    {
      return t.TotalSeconds.ToString();
    }
    private static TimeSpan parseTimeSpan(byte[] raw)
    {
      var str = Encoding.Default.GetString(raw);

      return TimeSpan.FromSeconds(long.Parse(str));
    }

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


    private void writeCmd(string command)
    {
      writeCore(w => w.WriteLine(command));
    }

    private void writeCmd(string command, string key)
    {
      writeCore(w => w.WriteLine(command + " " + key));
    }

    private void writeCmd(string command, string key, object param1)
    {
      writeCore(w => w.WriteLine("{0} {1} {2}".F(command, key, param1)));
    }

    private void writeCmd(string command, string key, object param1, object param2)
    {
      writeCore(w => w.WriteLine("{0} {1} {2} {3}".F(command, key, param1, param2)));
    }

    private void writeCmd(string command, string[] keys)
    {
      writeCore(w => w.WriteLine("{0} {1}".F(
        command, string.Join(" ", keys))));
    }

    private void writeCmd(string command, string key, string[] keys)
    {
      writeCore(w => w.WriteLine("{0} {1} {2}".F(
        command, key, string.Join(" ", keys))));
    }

    private void writeCmd(string command, string key, object param, string[] keys)
    {
      writeCore(w => w.WriteLine("{0} {1} {2} {3}".F(
        command, key, param, string.Join(" ", keys))));
    }


    private void writeValue(string command, string key, string value)
    {
      writeCore(w =>
      {
        var raw = encodeStr(value);

        w.WriteLine("{0} {1} {2}".F(command, key, raw.Length));
        w.WriteBulk(raw);
      });
    }

    private void writeValue(string command, string key, object param, string value)
    {
      writeCore(w =>
      {
        var raw = encodeStr(value);

        w.WriteLine("{0} {1} {2} {3}".F(command, key, param, raw.Length));
        w.WriteBulk(raw);
      });
    }


    private void writeMultiBulk(string command,
      IEnumerable<KeyValuePair<string, string>> kvPairs)
    {
      // TODO: ensure there aren't too many elements
      var keyValues = kvPairs.ToArray();

      writeCore(w =>
      {
        // TODO: Definitely should reuse the buffers here
        var buffer = encodeStr(command);

        w.WriteTypeChar(ResponseType.MultiBulk);
        w.WriteLine((keyValues.Length * 2) + 1);

        w.WriteTypeChar(ResponseType.Bulk);
        w.WriteLine(buffer.Length);
        w.WriteBulk(buffer);

        for (var i = 0; i < keyValues.Length; i++) {
          buffer = encodeStr(keyValues[i].Key);

          w.WriteTypeChar(ResponseType.Bulk);
          w.WriteLine(buffer.Length);
          w.WriteBulk(buffer);

          buffer = encodeStr(keyValues[i].Value);

          w.WriteTypeChar(ResponseType.Bulk);
          w.WriteLine(buffer.Length);
          w.WriteBulk(buffer);
        }
      });
    }

    private void writeMultiBulk(string command, string key,
      IEnumerable<KeyValuePair<string, string>> kvPairs)
    {
      var keyValues = kvPairs.ToArray();

      writeCore(w =>
      {
        var buffer = encodeStr(command);

        w.WriteTypeChar(ResponseType.MultiBulk);
        w.WriteLine((keyValues.Length * 2) + 2);

        w.WriteTypeChar(ResponseType.Bulk);
        w.WriteLine(buffer.Length);
        w.WriteBulk(buffer);

        buffer = encodeStr(key);

        w.WriteTypeChar(ResponseType.Bulk);
        w.WriteLine(buffer.Length);
        w.WriteBulk(buffer);

        for (var i = 0; i < keyValues.Length; i++) {
          buffer = encodeStr(keyValues[i].Key);

          w.WriteTypeChar(ResponseType.Bulk);
          w.WriteLine(buffer.Length);
          w.WriteBulk(buffer);

          buffer = encodeStr(keyValues[i].Value);

          w.WriteTypeChar(ResponseType.Bulk);
          w.WriteLine(buffer.Length);
          w.WriteBulk(buffer);
        }
      });
    }


    private int readInt()
    { return readCore(ResponseType.Integer, r => r.ReadNumberLine()); }

    private long readInt64()
    { return readCore(ResponseType.Integer, r => r.ReadNumberLine64()); }

    private bool readBool()
    { return readCore(ResponseType.Integer, r => r.ReadNumberLine() == 1); }

    private bool readOk() { return readStatus("OK"); }

    private bool readStatus(string expectedMsg)
    {
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


    private void writeCore(Action<RedisWriter> writeAction)
    {
      ensureNotDisposed();

      try {
        // TODO: Add pipelining support by recording writes
        // TODO: Add logging
        writeAction(_writer);

      }
      catch (Exception ex) {

        ensureClientState(ex);
        if (_disposed)
          throw;

        // usually, this catch block is run because of
        // idle connection timeouts from Redis side
        if (!_settings.ReconnectOnIdle)
          Dispose();

        // try again one more time before giving up
        try {
          Reset();
          writeAction(_writer);
        }
        catch (Exception ex_) {
          ensureClientState(ex_);
          throw;
        }
      }
    }

    private T readCore<T>(ResponseType expectedType, Func<RedisReader, T> readFunc)
    {
      ensureNotDisposed();

      try {
        // TODO: Add pipelining support by recording reads
        // TODO: Add logging
        // TODO: Add error-checking support to reads
        var type = _reader.ReadTypeChar();
        SAssert.ResponseType(expectedType, type);

        return readFunc(_reader);
      }
      catch (Exception ex) {
        ensureClientState(ex);
        throw;
      }
    }

    private void ensureClientState(Exception ex)
    {
      // TODO: Absorbing a generic IOException might be too dangerous.
      //       Multibulk operations may still cause the reader/writer into
      //       invalid states. e.g. First reads error, then absorbed, then
      //       other required bulk read skipped (but client is not disposed
      //       so user may issue more bulk commands and encounter parsing
      //       exceptions)
      if (!(ex is IOException || ex is ObjectDisposedException))
        Dispose();
    }
  }
}
