
using System;
using System.IO;
using System.Diagnostics;

namespace Sider
{
  public partial class RedisClient
  {
    public bool Ping()
    {
      _writer.WriteLine("PING");

      return _reader.ReadTypeChar() == ResponseType.SingleLine &&
        _reader.ReadStatusLine() == "PONG";
    }

    public int Del(params string[] keys)
    {
      _writer.WriteLine("DEL " + string.Join(" ", keys));

      var success = _reader.ReadTypeChar() == ResponseType.Integer;

      Assert.IsTrue(success, () => new ResponseException(
        "Issused a DEL but didn't receive an expected integer reply."));

      return _reader.ReadNumberLine();
    }

    public void Set(string key, byte[] value)
    {
      _writer.WriteLine(string.Format("SET {0} {1}", key, value.Length));
      _writer.WriteBulk(value);

      var success = _reader.ReadTypeChar() == ResponseType.SingleLine &&
        _reader.ReadStatusLine() == "OK";

      Assert.IsTrue(success, () => new ResponseException(
        "Issused a SET but didn't receive an expected OK message."));
    }

    public byte[] Get(string key)
    {
      _writer.WriteLine(string.Format("GET {0}", key));

      var success = _reader.ReadTypeChar() == ResponseType.Bulk;

      Assert.IsTrue(success, () => new ResponseException(
        "Issued a GET but didn't receive an expected bulk reply."));

      var length = _reader.ReadNumberLine();

      if (length > -1)
        return _reader.ReadBulk(length);

      return new byte[] { };
    }



  }
}
