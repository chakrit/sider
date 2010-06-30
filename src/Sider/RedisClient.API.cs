
using System.IO;

namespace Sider
{
  // see the redis commands reference for more info:
  // http://code.google.com/p/redis/wiki/CommandReference
  public partial class RedisClient
  {
    public bool Ping()
    {
      writeCore(w => w.WriteLine("PING"));
      return readStatus("PONG");
    }


    public int Del(params string[] keys)
    {
      writeCmd("DEL", keys);
      return readInt();
    }

    public string[] Keys(string pattern)
    {
      writeCmd("KEYS", pattern);
      return readMultiBulk();
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


    public string[] MGet(params string[] keys)
    {
      writeCmd("MGET", keys);
      return readMultiBulk();
    }


    public long Incr(string key)
    {
      writeCmd("INCR", key);
      return readInt64();
    }


    public int LPush(string key, string value)
    {
      writeCmd("LPUSH", key, value);
      return readInt();
    }

    public int RPush(string key, string value)
    {
      writeCmd("RPUSH", key, value);
      return readInt();
    }

    public string[] LRange(string key, int minInclusive, int maxInclusive)
    {
      writeListCmd("LRANGE", key, minInclusive, maxInclusive);
      return readMultiBulk();
    }

    public string LPop(string key)
    {
      writeCmd("LPOP", key);
      return readBulk();
    }

    public string RPop(string key)
    {
      writeCmd("RPOP", key);
      return readBulk();
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
      return readMultiBulk();
    }


    public bool ZAdd(string key, double score, string value)
    {
      writeZSetCmd("ZADD", key, score, value);
      return readBool();
    }

    public bool ZRem(string key, string value)
    {
      writeCmd("ZREM", key, value);
      return readBool();
    }

    public string[] ZRangeByScore(string key, double minInclusive, double maxInclusive)
    {
      writeZSetCmd("ZRANGEBYSCORE", key, minInclusive, maxInclusive);
      return readMultiBulk();
    }

    public int ZRemRangeByScore(string key, double minInclusive, double maxInclusive)
    {
      writeZSetCmd("ZREMRANGEBYSCORE", key, minInclusive, maxInclusive);
      return readInt();
    }

    public double ZIncrBy(string key, double amount, string value)
    {
      writeZSetCmd("ZINCRBY", key, amount, value);

      return readCore(ResponseType.Bulk, r =>
      {
        var length = _reader.ReadNumberLine();
        var raw = _reader.ReadBulk(length);

        return parseDouble(raw);
      });
    }
  }
}
