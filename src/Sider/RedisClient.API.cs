
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Sider
{
  // see the redis commands reference for more info:
  // http://redis.io/commands
  public partial class RedisClient : IRedisClient
  {
    #region Server

    public bool BgRewriteAOF()
    {
      writeCmd("BGREWRITEAOF");
      return readStatus("Background append only file rewriting started");
    }

    public bool BgSave()
    {
      writeCmd("BGSAVE");
      return readStatus("Background saving started");
    }

    public KeyValuePair<string, string>[] ConfigGet(string param)
    {
      writeCmd("CONFIG", "GET", param);
      return readKeyValues();
    }

    public bool ConfigSet(string param, string value)
    {
      writeCmd("CONFIG", "SET", param, value);
      return readOk();
    }

    public bool ConfigResetStat()
    {
      writeCmd("CONFIG", "RESETSTAT");
      return readOk();
    }

    public int DbSize()
    {
      writeCmd("DBSIZE");
      return readInt();
    }

    public string DebugObject(string key)
    {
      writeCmd("DEBUG", "OBJECT", key);
      return readStatus();
    }

    public void DebugSegfault()
    {
      writeCmd("DEBUG", "SEGFAULT");
      Dispose();
    }

    public bool FlushAll()
    {
      writeCmd("FLUSHALL");
      return readBool();
    }

    public bool FlushDb()
    {
      writeCmd("FLUSHDB");
      return readOk();
    }

    public DateTime LastSave()
    {
      writeCmd("LASTSAVE");
      return readCore(ResponseType.Integer, r =>
        parseDateTime(r.ReadNumberLine64()));
    }

    public bool Save()
    {
      writeCmd("SAVE");
      return readOk();
    }

    public void Shutdown()
    {
      writeCmd("SHUTDOWN");
      // TODO: Docs says error is possible but how to produce it?
      Dispose();
    }

    // NOTE: INFO, MONITOR, SLAVEOF, SYNC ... these commands are not implemented
    //   as the format is totally out-of-place and they are mostly used for
    //   manual server diagnostics/maintenance (not from a client lib)

    #endregion

    #region Connection

    public bool Auth(string password)
    {
      writeCmd("AUTH", password);
      return readOk();
    }

    public string Echo(string msg)
    {
      writeCmd("ECHO", msg);
      return readBulk();
    }

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

    public bool Select(int dbIndex)
    {
      writeCmd("SELECT", dbIndex.ToString());
      return readOk();
    }

    #endregion

    #region TODO: Transactions

    // TODO: Implement better transaction support
    //   queue up writeCore calls inside MULTI... EXEC
    //   and then process readCores in one go
    //   this should also enable nice pipelining commands support as well

    #endregion

    #region Keys

    public int Del(params string[] keys)
    {
      writeCmd("DEL", keys);
      return readInt();
    }

    public bool Exists(string key)
    {
      writeCmd("EXISTS", key);
      return readBool();
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

    public string[] Keys(string pattern)
    {
      writeCmd("KEYS", pattern);
      return readMultiBulk();
    }

    public bool Move(string key, int dbIndex)
    {
      writeCmd("MOVE", key, dbIndex.ToString());
      return readBool();
    }

    public bool Persist(string key)
    {
      writeCmd("PERSIST", key);
      return readBool();
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

    // TODO: Implement SORT complex arguments

    public TimeSpan TTL(string key)
    {
      writeCmd("TTL", key);
      return readCore(ResponseType.Integer,
        r => TimeSpan.FromSeconds(r.ReadNumberLine64()));
    }

    public RedisType Type(string key)
    {
      writeCmd("TYPE", key);
      return readCore(ResponseType.SingleLine, r =>
        RedisTypes.Parse(r.ReadStatusLine()));
    }

    #endregion

    #region Strings

    public int Append(string key, string value)
    {
      writeCmd("APPEND", key, value);
      return readInt();
    }

    public long Decr(string key)
    {
      writeCmd("DECR", key);
      return readInt64();
    }

    public long DecrBy(string key, long value)
    {
      writeCmd("DECRBY", key, value.ToString());
      return readInt64();
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
      return readBulkTo(target);
    }

    public int GetBit(string key, int offset)
    {
      writeCmd("GETBIT", key, offset.ToString());
      return readInt();
    }

    public string GetRange(string key, int start, int end)
    {
      writeCmd("GETRANGE", key, start.ToString(), end.ToString());
      return readBulk();
    }

    public byte[] GetRangeRaw(string key, int start, int end)
    {
      writeCmd("GETRANGE", key, start.ToString(), end.ToString());
      return readBulkRaw();
    }

    public int GetRangeTo(string key, int start, int end, Stream source)
    {
      writeCmd("GETRANGE", key, start.ToString(), end.ToString());
      return readBulkTo(source);
    }

    [Obsolete("This command has been renamed to GETRANGE as of Redis 2.0")]
    public string Substr(string key, int start, int end)
    {
      writeCmd("SUBSTR", key, start.ToString(), end.ToString());
      return readBulk();
    }

    public string GetSet(string key, string value)
    {
      writeCmd("GETSET", key, value);
      return readBulk();
    }

    public long Incr(string key)
    {
      writeCmd("INCR", key);
      return readInt64();
    }

    public long IncrBy(string key, long value)
    {
      writeCmd("INCRBY", key, value.ToString());
      return readInt64();
    }

    public string[] MGet(params string[] keys)
    {
      writeCmd("MGET", keys);
      return readMultiBulk();
    }

    public bool MSet(IEnumerable<KeyValuePair<string, string>> mappings)
    {
      writeCmd("MSET", mappings.ToArray());
      return readOk();
    }

    public bool MSetNX(IEnumerable<KeyValuePair<string, string>> mappings)
    {
      writeCmd("MSETNX", mappings.ToArray());
      return readBool();
    }

    public bool Set(string key, string value)
    {
      writeCmd("SET", key, value);
      return readOk();
    }

    public bool SetRaw(string key, byte[] raw)
    {
      writeCmd("SET", key, raw);
      return readOk();
    }

    public bool SetFrom(string key, Stream source, int count)
    {
      writeCmd("SET", key, source, count);
      return readOk();
    }

    public int SetBit(string key, int offset, int value)
    {
      writeCmd("SETBIT", key, offset.ToString(), value.ToString());
      return readInt();
    }

    public bool SetEX(string key, TimeSpan ttl, string value)
    {
      writeCmd("SETEX", key, formatTimeSpan(ttl), value);
      return readOk();
    }

    public bool SetNX(string key, string value)
    {
      writeCmd("SETNX", key, value);
      return readBool();
    }

    public int SetRange(string key, int offset, string value)
    {
      writeCmd("SETRANGE", key, offset.ToString(), value);
      return readInt();
    }

    public int SetRangeRaw(string key, int offset, byte[] value)
    {
      writeCmd("SETRANGE", key, offset.ToString(), value);
      return readInt();
    }

    public int SetRangeFrom(string key, int offset, Stream source, int count)
    {
      writeCmd("SETRANGE", key, offset.ToString(), source, count);
      return readInt();
    }

    public int Strlen(string key)
    {
      writeCmd("STRLEN", key);
      return readInt();
    }

    #endregion

    #region Hashes

    public bool HDel(string key, string field)
    {
      writeCmd("HDEL", key, field);
      return readBool();
    }

    public bool HExists(string key, string field)
    {
      writeCmd("HEXISTS", key, field);
      return readBool();
    }

    public string HGet(string key, string field)
    {
      writeCmd("HGET", key, field);
      return readBulk();
    }

    public byte[] HGetRaw(string key, string field)
    {
      writeCmd("HGET", key, field);
      return readBulkRaw();
    }

    public int HGetTo(string key, string field, Stream target)
    {
      writeCmd("HGET", key, field);
      return readCore(ResponseType.Bulk, r =>
      {
        var length = r.ReadNumberLine();
        if (length > -1)
          r.ReadBulkTo(target, length);

        return length;
      });
    }

    public IEnumerable<KeyValuePair<string, string>> HGetAll(string key)
    {
      writeCmd("HGETALL", key);
      return readKeyValues();
    }

    public long HIncrBy(string key, string field, long amount)
    {
      writeCmd("HINCRBY", key, field, amount.ToString());
      return readInt64();
    }

    public string[] HKeys(string key)
    {
      writeCmd("HKEYS", key);
      return readMultiBulk();
    }

    public int HLen(string key)
    {
      writeCmd("HLEN", key);
      return readInt();
    }

    public string[] HMGet(string key, params string[] fields)
    {
      writeCmd("HMGET", key, fields);
      return readMultiBulk();
    }

    public bool HMSet(string key, IEnumerable<KeyValuePair<string, string>> mappings)
    {
      writeCmd("HMSET", key, mappings.ToArray());
      return readOk();
    }

    public bool HSet(string key, string field, string value)
    {
      writeCmd("HSET", key, field, value);
      return readBool();
    }

    public bool HSetRaw(string key, string field, byte[] data)
    {
      writeCmd("HSET", key, field, data);
      return readBool();
    }

    public bool HSetFrom(string key, string field, Stream source, int count)
    {
      writeCmd("HSET", key, field, source, count);
      return readBool();
    }

    public bool HSetNX(string key, string field, string value)
    {
      writeCmd("HSETNX", key, field, value);
      return readBool();
    }

    public string[] HVals(string key)
    {
      writeCmd("HVALS", key);
      return readMultiBulk();
    }

    #endregion

    #region Lists

    // TODO: Implement blocking commands
    //   BLPOP, BRPOP, BRPOPLPUSH

    public string LIndex(string key, int index)
    {
      writeCmd("LINDEX", key, index.ToString());
      return readBulk();
    }

    // TODO: Implement LInsert with complex args

    public int LLen(string key)
    {
      writeCmd("LLEN", key);
      return readInt();
    }

    public string LPop(string key)
    {
      writeCmd("LPOP", key);
      return readBulk();
    }

    public int LPush(string key, string value)
    {
      writeCmd("LPUSH", key, value);
      return readInt();
    }

    public int LPushX(string key, string value)
    {
      writeCmd("LPUSHX", key, value);
      return readInt();
    }

    public string[] LRange(string key, int minIncl, int maxIncl)
    {
      writeCmd("LRANGE", key, minIncl.ToString(), maxIncl.ToString());
      return readMultiBulk();
    }

    public int LRem(string key, int count, string value)
    {
      writeCmd("LREM", key, count.ToString(), value);
      return readInt();
    }

    public bool LSet(string key, int index, string value)
    {
      writeCmd("LSET", key, index.ToString(), value);
      return readOk();
    }

    public bool LTrim(string key, int minIncl, int maxIncl)
    {
      writeCmd("LTRIM", key, minIncl.ToString(), maxIncl.ToString());
      return readOk();
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

    public int RPush(string key, string value)
    {
      writeCmd("RPUSH", key, value);
      return readInt();
    }

    public int RPushX(string key, string value)
    {
      writeCmd("RPUSHX", key, value);
      return readInt();
    }

    #endregion

    #region Sets

    public bool SAdd(string key, string value)
    {
      writeCmd("SADD", key, value);
      return readBool();
    }

    public int SCard(string key)
    {
      writeCmd("SCARD", key);
      return readInt();
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

    public bool SIsMember(string key, string value)
    {
      writeCmd("SISMEMBER", key, value);
      return readBool();
    }

    public string[] SMembers(string key)
    {
      writeCmd("SMEMBERS", key);
      return readMultiBulk();
    }

    public bool SMove(string srcKey, string destKey, string value)
    {
      writeCmd("SMOVE", srcKey, destKey, value);
      return readBool();
    }

    public string SPop(string key)
    {
      writeCmd("SPOP", key);
      return readBulk();
    }

    public string SRandMember(string key)
    {
      writeCmd("SRANDMEMBER", key);
      return readBulk();
    }

    public bool SRem(string key, string value)
    {
      writeCmd("SREM", key, value);
      return readBool();
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

    #endregion

    #region Sorted Sets

    public bool ZAdd(string key, double score, string value)
    {
      writeCmd("ZADD", key, formatDouble(score), value);
      return readBool();
    }

    public int ZCard(string key)
    {
      writeCmd("ZCARD", key);
      return readInt();
    }

    public int ZCount(string key, double minIncl, double maxIncl)
    {
      writeCmd("ZCOUNT", key, formatDouble(minIncl), formatDouble(maxIncl));
      return readInt();
    }

    public double ZIncrBy(string key, double amount, string value)
    {
      writeCmd("ZINCRBY", key, formatDouble(amount), value);
      return readDouble();
    }

    public int ZInterStore(string destKey, params string[] srcKeys)
    {
      writeCmd("ZINTERSTORE", destKey, srcKeys.Length.ToString(), srcKeys);
      return readInt();
    }

    public string[] ZRange(string key, int startRank, int endRank)
    {
      writeCmd("ZRANGE", key, startRank.ToString(), endRank.ToString());
      return readMultiBulk();
    }

    public string[] ZRangeByScore(string key, double minIncl, double maxIncl)
    {
      writeCmd("ZRANGEBYSCORE", key, formatDouble(minIncl), formatDouble(maxIncl));
      return readMultiBulk();
    }

    public int ZRank(string key, string value)
    {
      writeCmd("ZRANK", key, value);
      return readInt();
    }

    public bool ZRem(string key, string value)
    {
      writeCmd("ZREM", key, value);
      return readBool();
    }

    public int ZRemRangeByRank(string key, int startRank, int endRank)
    {
      writeCmd("ZREMRANGEBYRANK", key, startRank.ToString(), endRank.ToString());
      return readInt();
    }

    public int ZRemRangeByScore(string key, double minIncl, double maxIncl)
    {
      writeCmd("ZREMRANGEBYSCORE", key, formatDouble(minIncl), formatDouble(maxIncl));
      return readInt();
    }

    public string[] ZRevRange(string key, int startRank, int endRank)
    {
      writeCmd("ZREVRANGE", key, startRank.ToString(), endRank.ToString());
      return readMultiBulk();
    }

    public string[] ZRevRangeByScore(string key, double minIncl, double maxIncl)
    {
      writeCmd("ZREVRANGE", key, formatDouble(minIncl), formatDouble(maxIncl));
      return readMultiBulk();
    }

    public int ZRevRank(string key, string value)
    {
      writeCmd("ZREVRANK", key, value);
      return readInt();
    }

    public double ZScore(string key, string value)
    {
      writeCmd("ZSCORE", key);
      return readDouble();
    }

    public int ZUnionStore(string destKey, params string[] srcKeys)
    {
      writeCmd("ZUNIONSTORE", destKey, srcKeys.Length.ToString(), srcKeys);
      return readInt();
    }

    #endregion

    #region TODO: Pub/Sub

    // Could be done by using Observables
    // the tricky part will be how to implement unsubscribe?

    #endregion
  }
}
