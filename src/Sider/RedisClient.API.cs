
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
      _writer.WriteLine("DEL {0}".F(string.Join(" ", keys)));

      var success = _reader.ReadTypeChar() == ResponseType.Integer;

      Assert.IsTrue(success, () => new ResponseException(
        "Issused a DEL but didn't receive an expected integer reply."));

      return _reader.ReadNumberLine();
    }

    public bool Set(string key, byte[] value)
    {
      _writer.WriteLine("SET {0} {1}".F(key, value.Length));
      _writer.WriteBulk(value);

      // TODO: Handle error cases
      return _reader.ReadTypeChar() == ResponseType.SingleLine &&
        _reader.ReadStatusLine() == "OK";
    }

    public byte[] Get(string key)
    {
      _writer.WriteLine("GET {0}".F(key));

      var success = _reader.ReadTypeChar() == ResponseType.Bulk;

      Assert.IsTrue(success, () => new ResponseException(
        "Issued a GET but didn't receive an expected bulk reply."));

      var length = _reader.ReadNumberLine();

      return length > -1 ?
        _reader.ReadBulk(length) :
        new byte[] { };
    }

    public long Incr(string key)
    {
      _writer.WriteLine("INCR {0}".F(key));

      var success = _reader.ReadTypeChar() == ResponseType.Integer;

      Assert.IsTrue(success, () => new ResponseException(
        "Issued a GET but didn't receive an expected integer reply."));

      return _reader.ReadNumberLine64();
    }


  }
}
