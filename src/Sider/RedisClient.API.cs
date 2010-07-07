
using System;
using System.IO;

namespace Sider
{
  // see the redis commands reference for more info:
  // http://code.google.com/p/redis/wiki/CommandReference
  public partial class RedisClient
  {

    #region Connection Handling

    public bool Ping()
    {
      writeCmd("PING");
      return readStatus("PONG");
    }

    public void Quit()
    {
      writeCmd("QUIT");
      Dispose();
    }

    public bool Auth(string password)
    {
      writeCmd("AUTH", password);
      return readOk();
    }

    #endregion

    #region Commands operating on all kinds of values

    public bool Exists(string key)
    {
      writeCmd("EXISTS", key);
      return readBool();
    }

    public int Del(params string[] keys)
    {
      writeCmd("DEL", keys);
      return readInt();
    }

    public RedisType Type(string key)
    {
      writeCmd("TYPE", key);
      return readCore(ResponseType.SingleLine, r =>
      {
        // TODO: Eliminate this switch (not sure if Enum.Parse will be slow)
        switch (r.ReadStatusLine()) {
          case "string": return RedisType.String;
          case "list": return RedisType.List;
          case "set": return RedisType.Set;
          case "zset": return RedisType.ZSet;
          case "hash": return RedisType.Hash;

          //case "none": 
          default: return RedisType.None;
        }
      });
    }

    public string[] Keys(string pattern)
    {
      writeCmd("KEYS", pattern);
      return readMultiBulk();
    }

    public string RandomKey()
    {
      writeCmd("RANDOMKEY");
      return readBulk();
    }

    public bool Rename(string oldKey, string newKey)
    {
      writeCmd("RENAME", oldKey, newKey);
      return readOk();
    }

    public bool RenameNX(string oldKey, string newKey)
    {
      writeCmd("RENAMENX", oldKey, newKey);
      return readBool();
    }


    public int DbSize()
    {
      writeCmd("DBSIZE");
      return readInt();
    }

    public bool Expire(string key, TimeSpan span)
    {
      writeCmd("EXPIRE", key, formatTimeSpan(span));
      return readBool();
    }

    public bool ExpireAt(string key, DateTime time)
    {
      writeCmd("EXPIREAT", key, formatDateTime(time));
      return readBool();
    }

    public TimeSpan TTL(string key)
    {
      writeCmd("TTL", key);
      return readCore(ResponseType.Integer, r =>
        TimeSpan.FromSeconds(r.ReadNumberLine64()));
    }


    public bool Select(int dbIndex)
    {
      writeCmd("SELECT", dbIndex.ToString());
      return readBool();
    }

    public bool Move(string key, int dbIndex)
    {
      writeCmd("MOVE", key, dbIndex);
      return readBool();
    }

    public bool FlushDB()
    {
      writeCmd("FLUSHDB");
      return readOk();
    }

    public bool FlushAll()
    {
      writeCmd("FLUSHALL");
      return readBool();
    }

    #endregion

    #region Commands operating on string values

    public bool Set(string key, string value)
    {
      writeValue("SET", key, value);
      return readOk();
    }

    public bool SetRaw(string key, byte[] raw)
    {
      writeCore(w =>
      {
        w.WriteLine("SET {0} {1}".F(key, raw.Length));
        w.WriteBulk(raw);
      });
      return readOk();
    }

    public bool SetFrom(string key, Stream source, int count)
    {
      writeCore(w =>
      {
        w.WriteLine("SET {0} {1}".F(key, count));
        w.WriteBulkFrom(source, count);
      });
      return readOk();
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


    public string GetSet(string key, string value)
    {
      writeValue("GETSET", key, value);
      return readBulk();
    }

    public string[] MGet(params string[] keys)
    {
      writeCmd("MGET", keys);
      return readMultiBulk();
    }


    public bool SetNX(string key, string value)
    {
      writeValue("SETNX", key, value);
      return readBool();
    }

    public bool SetEX(string key, TimeSpan ttl, string value)
    {
      writeValue("SETEX", key, formatTimeSpan(ttl), value);
      return readOk();
    }


    public long Incr(string key)
    {
      writeCmd("INCR", key);
      return readInt64();
    }

    public long IncrBy(string key, long value)
    {
      writeCmd("INCRBY", key, value);
      return readInt64();
    }

    public long Decr(string key)
    {
      writeCmd("DECR", key);
      return readInt64();
    }

    public long DecrBy(string key, long value)
    {
      writeCmd("DECRBY", key, value);
      return readInt64();
    }


    public int Append(string key, string value)
    {
      writeValue("APPEND", key, value);
      return readInt();
    }

    public string Substr(string key, int start, int end)
    {
      writeCmd("SUBSTR", key, start, end);
      return readBulk();
    }

    #endregion

    #region Commands operating on lists

    public int RPush(string key, string value)
    {
      writeValue("RPUSH", key, value);
      return readInt();
    }

    public int LPush(string key, string value)
    {
      writeValue("LPUSH", key, value);
      return readInt();
    }


    public int LLen(string key)
    {
      writeCmd("LLEN", key);
      return readInt();
    }

    public string[] LRange(string key, int minIncl, int maxIncl)
    {
      writeCmd("LRANGE", key, minIncl, maxIncl);
      return readMultiBulk();
    }

    public bool LTrim(string key, int minIncl, int maxIncl)
    {
      writeCmd("LTRIM", key, minIncl, maxIncl);
      return readOk();
    }


    public string LIndex(string key, int index)
    {
      writeCmd("LINDEX", key, index);
      return readBulk();
    }

    public bool LSet(string key, int index, string value)
    {
      writeValue("LSET", key, index, value);
      return readOk();
    }

    public int LRem(string key, int count, string value)
    {
      writeValue("LREM", key, count, value);
      return readInt();
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

    public string RPopLPush(string srcKey, string destKey)
    {
      writeCmd("RPOPLPUSH", srcKey, destKey);
      return readBulk();
    }

    #endregion

    #region Commands operating on sets

    public bool SAdd(string key, string value)
    {
      writeValue("SADD", key, value);
      return readBool();
    }

    public bool SRem(string key, string value)
    {
      writeValue("SREM", key, value);
      return readBool();
    }

    public string SPop(string key)
    {
      writeCmd("SPOP", key);
      return readBulk();
    }

    public bool SMove(string srcKey, string destKey, string value)
    {
      writeValue("SMOVE", srcKey, destKey, value);
      return readBool();
    }


    public int SCard(string key)
    {
      writeCmd("SCARD", key);
      return readInt();
    }

    public bool SIsMember(string key, string value)
    {
      writeValue("SISMEMBER", key, value);
      return readBool();
    }


    public string[] SInter(params string[] keys)
    {
      writeCmd("SINTER", keys);
      return readMultiBulk();
    }

    public bool SInterStore(string destKey, params string[] keys)
    {
      writeCmd("SINTERSTORE", destKey, keys);
      return readOk();
    }

    public string[] SUnion(params string[] keys)
    {
      writeCmd("SUNION", keys);
      return readMultiBulk();
    }

    public bool SUnionStore(string destKey, params string[] keys)
    {
      writeCmd("SUNIONSTORE", destKey, keys);
      return readOk();
    }

    public string[] SDiff(params string[] keys)
    {
      writeCmd("SDIFF", keys);
      return readMultiBulk();
    }

    public bool SDiffStore(string destKey, params string[] keys)
    {
      writeCmd("SDIFFSTORE", destKey, keys);
      return readOk();
    }


    public string[] SMembers(string key)
    {
      writeCmd("SMEMBERS", key);
      return readMultiBulk();
    }

    public string SRandMember(string key)
    {
      writeCmd("SRANDMEMBER", key);
      return readBulk();
    }

    #endregion

    #region Commands operating on sorted sets

    public bool ZAdd(string key, double score, string value)
    {
      writeValue("ZADD", key, formatDouble(score), value);
      return readBool();
    }

    public bool ZRem(string key, string value)
    {
      writeValue("ZREM", key, value);
      return readBool();
    }

    public double ZIncrBy(string key, double amount, string value)
    {
      writeValue("ZINCRBY", key, formatDouble(amount), value);
      return readDouble();
    }


    public int ZRank(string key, string value)
    {
      writeValue("ZRANK", key, value);
      return readInt();
    }

    public int ZRevRank(string key, string value)
    {
      writeValue("ZREVRANK", key, value);
      return readInt();
    }


    public string[] ZRange(string key, int startRank, int endRank)
    {
      writeCmd("ZRANGE", key, startRank, endRank);
      return readMultiBulk();
    }

    public string[] ZRevRange(string key, int startRank, int endRank)
    {
      writeCmd("ZREVRANGE", key, startRank, endRank);
      return readMultiBulk();
    }


    public string[] ZRangeByScore(string key, double minIncl, double maxIncl)
    {
      writeCmd("ZRANGEBYSCORE", key, formatDouble(minIncl), formatDouble(maxIncl));
      return readMultiBulk();
    }


    public int ZRemRangeByRank(string key, int startRank, int endRank)
    {
      writeCmd("ZREMRANGEBYRANK", key, startRank, endRank);
      return readInt();
    }

    public int ZRemRangeByScore(string key, double minIncl, double maxIncl)
    {
      writeCmd("ZREMRANGEBYRANK", key, formatDouble(minIncl), formatDouble(maxIncl));
      return readInt();
    }


    public int ZCard(string key)
    {
      writeCmd("ZCARD", key);
      return readInt();
    }

    public double ZScore(string key, string value)
    {
      writeCmd("ZSCORE", key);
      return readDouble();
    }


    public int ZUnionStore(string destKey, params string[] srcKeys)
    {
      writeCmd("ZUNIONSTORE", destKey, srcKeys.Length, srcKeys);
      return readInt();
    }

    public int ZInterStore(string destKey, params string[] srcKeys)
    {
      writeCmd("ZINTERSTORE", destKey, srcKeys.Length, srcKeys);
      return readInt();
    }

    #endregion
  }
}
