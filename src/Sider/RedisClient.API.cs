
using System.IO;

namespace Sider
{
  public partial class RedisClient
  {
    public bool Ping()
    {
      writeCore(w => w.WriteLine("PING"));
      return readStatus("PONG");
    }


    public int Del(params string[] keys)
    {
      writeCmd("DEL", string.Join(" ", keys));
      return readInt();
    }

    public bool Exists(string key)
    {
      writeCmd("EXISTS", key);
      return readBool();
    }


    public bool Set(string key, string value)
    {
      writeCmd("SET", key, value);
      return readStatus("OK");
    }

    public bool SetRaw(string key, byte[] raw)
    {
      writeCmd("SET", key, raw);
      return readStatus("OK");
    }

    public bool SetFrom(string key, Stream source, int count)
    {
      writeCore(w =>
      {
        w.WriteLine("SET {0} {1}".F(key, count));
        w.WriteBulkFrom(source, count);
      });
      return readStatus("OK");
    }


    public bool SetNX(string key, string value)
    {
      writeCmd("SETNX", key, value);
      return readBool();
    }


    public string Get(string key)
    {
      writeCmd("GET", key);
      return readBulk();
    }

    public byte[] GetRaw(string key)
    {
      writeCmd("GET", key);
      return readBulkRaw();
    }

    public int GetTo(string key, Stream target)
    {
      writeCmd("GET", key);
      return readCore(ResponseType.Bulk, r =>
      {
        var length = r.ReadNumberLine();
        if (length > -1)
          r.ReadBulkTo(target, length);

        return length;
      });
    }


    public long Incr(string key)
    {
      writeCmd("INCR", key);
      return readInt64();
    }


    public bool SAdd(string key, string value)
    {
      writeCmd("SADD", key, value);
      return readBool();
    }

    public bool SRem(string key, string value)
    {
      writeCmd("SREM", key, value);
      return readBool();
    }

    public string[] SMembers(string key)
    {
      writeCmd("SMEMBERS", key);

      return readCore(ResponseType.MultiBulk, r =>
      {
        var bulksCount = _reader.ReadNumberLine();
        var result = new string[bulksCount];

        for (var i = 0; i < bulksCount; i++) {
          var type = _reader.ReadTypeChar();
          Assert.ResponseType(ResponseType.Bulk, type);

          var length = _reader.ReadNumberLine();
          result[i] = decodeStr(_reader.ReadBulk(length));
        }

        return result;
      });
    }


    public bool ZAdd(string key, float score, string value)
    {
      writeCmd("ZADD", key, score, value);
      return readBool();
    }

    public bool ZRem(string key, string value)
    {
      writeCmd("ZREM", key, value);
      return readBool();
    }

    public int ZRemRangeByScore(string key, float minInclusive, float maxInclusive)
    {
      writeCmd("ZREMRANGEBYSCORE", key, minInclusive, maxInclusive);
      return readInt();
    }

    public float ZIncrBy(string key, float amount, string value)
    {
      writeCmd("ZINCRBY", key, amount, value);

      return readCore(ResponseType.Bulk, r =>
      {
        var length = _reader.ReadNumberLine();
        var raw = _reader.ReadBulk(length);

        return parseFloat(raw);
      });
    }
  }
}
