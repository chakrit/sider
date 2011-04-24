
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Sider
{
  public partial class InvocationBuilder<T>
  {
    #region Server

    public Invocation<bool> BgRewriteAof()
    {
      return invoke("BGREWRITEAOF",
        r => r.ReadStatus("Background append only file rewriting started"));
    }

    public Invocation<bool> BgSave()
    {
      return invoke("BGSAVE", r => r.ReadStatus("Background saving started"));
    }

    public Invocation<KeyValuePair<string, string>[]> ConfigGet(string param)
    {
      return invoke("CONFIG", 2,
        w => { w.WriteArg("GET"); w.WriteArg(param); },
        r => r.ReadStrKeyValues());
    }

    public Invocation<bool> ConfigSet(string param, string value)
    {
      return invoke("CONFIG", 3,
        w => { w.WriteArg("SET"); w.WriteArg(param); w.WriteArg(value); },
        r => r.ReadOk());
    }

    public Invocation<bool> ConfigResetStat()
    {
      return invoke("CONFIG", "RESETSTAT", r => r.ReadOk());
    }

    public Invocation<int> DbSize()
    {
      return invoke("DBSIZE", r => r.ReadInt());
    }

    public Invocation<string> DebugObject(string key)
    {
      return invoke("DEBUG", 2,
        w => { w.WriteArg("OBJECT"); w.WriteArg(key); },
        r => r.ReadStatus());
    }

    public Invocation<object> DebugSegfault()
    {
      return invoke("DEBUG", "SEGFAULT", r => (object)null);
    }

    public Invocation<bool> FlushAll()
    {
      return invoke("FLUSHALL", r => r.ReadOk());
    }

    public Invocation<bool> FlushDb()
    {
      return invoke("FLUSHDB", r => r.ReadOk());
    }

    public Invocation<DateTime> LastSave()
    {
      return invoke("LASTSAVE", r => r.ReadDateTime());
    }

    public Invocation<bool> Save()
    {
      return invoke("SAVE", r => r.ReadOk());
    }

    public Invocation<object> Shutdown()
    {
      return invoke("SHUTDOWN", r => (object)null);
    }

    // TODO: section support
    public Invocation<KeyValuePair<string, string>[]> Info()
    {
      return invoke("INFO", r =>
      {
        var rawResult = r.ReadStrBulk()
          .Split(new[] { '\r', '\n', ':' }, StringSplitOptions.RemoveEmptyEntries);

        var result = new KeyValuePair<string, string>[rawResult.Length / 2];
        var resultIdx = 0;
        for (var i = 0; i < rawResult.Length; i += 2)
          result[resultIdx++] = new KeyValuePair<string, string>(
            rawResult[i], rawResult[i + 1]);

        return result;
      });
    }

    public Invocation<bool> SlaveOf(string host, int port)
    {
      return invoke("SLAVEOF", 2,
        w => { w.WriteArg(host); w.WriteArg(port); },
        r => r.ReadOk());
    }

    #endregion

    #region Connection

    public Invocation<bool> Auth(string password)
    {
      return invoke("AUTH", password, r => r.ReadOk());
    }

    public Invocation<string> Echo(string msg)
    {
      return invoke("ECHO", msg, r => r.ReadStrBulk());
    }

    public Invocation<T> Echo(T msg)
    {
      return invoke("ECHO", 1,
        w => w.WriteArg(_serializer, msg),
        _readObj);
    }

    public Invocation<bool> Ping()
    {
      return invoke("PING", 0,
        w => { },
        r => r.ReadStatus("PONG"));
    }

    public Invocation<object> Quit()
    {
      // TODO: Dispose the client?
      return invoke("QUIT", r => (object)null);
    }

    public Invocation<bool> Select(int dbIndex)
    {
      return invoke("SELECT", 1,
        w => w.WriteArg(dbIndex),
        r => r.ReadOk());
    }

    #endregion

    #region Transactions

    public Invocation<bool> Multi()
    {
      return invoke("MULTI", r => r.ReadOk());
    }

    public Invocation<bool> Discard()
    {
      return invoke("DISCARD", r => r.ReadOk());
    }

    public Invocation<IEnumerable<object>> Exec()
    {
      return invoke<IEnumerable<object>>("EXEC", r =>
      {
        throw new Exception("EXEC requires a TransactedExecutor to function.");
      });
    }

    public Invocation<bool> Watch(params string[] keys)
    {
      return invoke("WATCH", keys.Length,
        w => Array.ForEach(keys, w.WriteArg),
        r => r.ReadOk());
    }

    public Invocation<bool> Unwatch()
    {
      return invoke("UNWATCH", r => r.ReadOk());
    }

    #endregion

    #region Keys

    public Invocation<int> Del(params string[] keys)
    {
      return invoke("DEL", keys.Length,
        w => Array.ForEach(keys, w.WriteArg),
        r => r.ReadInt());
    }

    public Invocation<bool> Exists(string key)
    {
      return invoke("EXISTS", key, r => r.ReadBool());
    }

    public Invocation<bool> Expire(string key, TimeSpan span)
    {
      return invoke("EXPIRE", 2,
        w => { w.WriteArg(key); w.WriteArg(span); },
        r => r.ReadBool());
    }

    public Invocation<bool> ExpireAt(string key, DateTime time)
    {
      return invoke("EXPIREAT", 2,
        w => { w.WriteArg(key); w.WriteArg(time); },
        r => r.ReadBool());
    }

    public Invocation<string[]> Keys(string pattern)
    {
      return invoke("KEYS", pattern, r => r.ReadStrMultiBulk());
    }

    public Invocation<bool> Move(string key, int dbIndex)
    {
      return invoke("MOVE", 2,
        w => { w.WriteArg(key); w.WriteArg(dbIndex); },
        r => r.ReadBool());
    }

    public Invocation<bool> Persist(string key)
    {
      return invoke("PERSIST", r => r.ReadBool());
    }

    public Invocation<string> RandomKey()
    {
      return invoke("RANDOMKEY", r => r.ReadStrBulk());
    }

    public Invocation<bool> Rename(string oldKey, string newKey)
    {
      return invoke("RENAME", 2,
        w => { w.WriteArg(oldKey); w.WriteArg(newKey); },
        r => r.ReadOk());
    }

    public Invocation<bool> RenameNX(string oldKey, string newKey)
    {
      return invoke("RENAMENX", 2,
        w => { w.WriteArg(oldKey); w.WriteArg(newKey); },
        r => r.ReadBool());
    }

    // SORT key [BY pattern] [LIMIT offset count] [GET pattern [GET pattern ...]]
    //     [ASC|DESC] [ALPHA] [STORE destination]
    public Invocation<T[]> Sort(string key, string byPattern = null,
      int? limitOffset = null, int? limitCount = null,
      string[] getPattern = null, bool descending = false,
      bool alpha = false, string store = null)
    {
      var items = new List<string>();
      items.Add(key);

      if (!string.IsNullOrEmpty(byPattern)) {
        items.Add("BY");
        items.Add(byPattern);
      }

      if (limitOffset.HasValue || limitCount.HasValue) {
        items.Add("LIMIT");
        items.Add(limitOffset.GetValueOrDefault(0).ToString());
        items.Add(limitCount.GetValueOrDefault(int.MaxValue).ToString());
      }

      if (getPattern != null) {
        foreach (var pattern in getPattern) {
          items.Add("GET");
          items.Add(pattern);
        }
      }

      if (descending) items.Add("DESC");
      if (alpha) items.Add("ALPHA");

      if (!string.IsNullOrEmpty(store)) {
        items.Add("STORE");
        items.Add(store);
      }

      return invoke("SORT", items.Count,
        w => { items.ForEach(w.WriteArg); },
        _readObjs);
    }

    public Invocation<TimeSpan> TTL(string key)
    {
      return invoke("TTL", key, r => r.ReadTimeSpan());
    }

    public Invocation<RedisType> Type(string key)
    {
      return invoke("TYPE", key, r => RedisTypes.Parse(r.ReadStatus()));
    }

    #endregion

    #region Strings

    public Invocation<int> Append(string key, T value)
    {
      return invoke("APPEND", key, value, r => r.ReadInt());
    }

    public Invocation<long> Decr(string key)
    {
      return invoke("DECR", key, r => r.ReadInt64());
    }

    public Invocation<long> DecrBy(string key, long value)
    {
      return invoke("DECRBY", 2,
        w => { w.WriteArg(key); w.WriteArg(value); },
        r => r.ReadInt64());
    }

    public Invocation<T> Get(string key)
    {
      return invoke("GET", key, _readObj);
    }

    public Invocation<byte[]> GetRaw(string key)
    {
      return invoke("GET", key, r => r.ReadRawBulk());
    }

    public Invocation<int> GetTo(string key, Stream target)
    {
      return invoke("GET", key, r => r.ReadStreamedBulk(target));
    }

    public Invocation<int> GetBit(string key, int offset)
    {
      return invoke("GETBIT", 2,
        w => { w.WriteArg(key); w.WriteArg(offset); },
        r => r.ReadInt());
    }

    public Invocation<T> GetRange(string key, int start, int end)
    {
      return invoke("GETRANGE", 3,
        w => { w.WriteArg(key); w.WriteArg(start); w.WriteArg(end); },
        _readObj);
    }

    public Invocation<byte[]> GetRangeRaw(string key, int start, int end)
    {
      return invoke("GETRANGE", 3,
        w => { w.WriteArg(key); w.WriteArg(start); w.WriteArg(end); },
        r => r.ReadRawBulk());
    }

    public Invocation<int> GetRangeTo(string key, int start, int end, Stream target)
    {
      return invoke("GETRANGE", 3,
        w => { w.WriteArg(key); w.WriteArg(start); w.WriteArg(end); },
        r => r.ReadStreamedBulk(target));
    }

    public Invocation<T> GetSet(string key, T value)
    {
      return invoke("GETSET", key, value, _readObj);
    }

    public Invocation<long> Incr(string key)
    {
      return invoke("INCR", key, r => r.ReadInt64());
    }

    public Invocation<long> IncrBy(string key, long value)
    {
      return invoke("INCRBY", 2,
        w => { w.WriteArg(key); w.WriteArg(value); },
        r => r.ReadInt64());
    }

    public Invocation<T[]> MGet(params string[] keys)
    {
      return invoke("MGET", keys.Length,
        w => Array.ForEach(keys, w.WriteArg),
        _readObjs);
    }

    public Invocation<bool> MSet(IEnumerable<KeyValuePair<string, T>> mappings)
    {
      var arr = mappings.ToArray();
      return invoke("MSET", arr.Length * 2,
        w => Array.ForEach(arr, kv =>
        {
          w.WriteArg(kv.Key);
          w.WriteArg(_serializer, kv.Value);
        }),
        r => r.ReadOk());
    }

    public Invocation<bool> MSetNX(IEnumerable<KeyValuePair<string, T>> mappings)
    {
      var arr = mappings.ToArray();
      return invoke("MSETNX", arr.Length * 2,
        w => Array.ForEach(arr, kv =>
        {
          w.WriteArg(kv.Key);
          w.WriteArg(_serializer, kv.Value);
        }),
        r => r.ReadOk());
    }

    public Invocation<bool> Set(string key, T value)
    {
      return invoke("SET", key, value, r => r.ReadOk());
    }

    public Invocation<bool> SetRaw(string key, byte[] raw)
    {
      return invoke("SET", 2,
        w => { w.WriteArg(key); w.WriteArg(raw); },
        r => r.ReadOk());
    }

    public Invocation<bool> SetFrom(string key, Stream source, int count)
    {
      return invoke("SET", 2,
        w => { w.WriteArg(key); w.WriteArg(source, count); },
        r => r.ReadOk());
    }

    public Invocation<int> SetBit(string key, int offset, int value)
    {
      return invoke("SETBIT", 3,
        w => { w.WriteArg(key); w.WriteArg(offset); w.WriteArg(value); },
        r => r.ReadInt());
    }

    public Invocation<bool> SetEX(string key, TimeSpan ttl, T value)
    {
      return invoke("SETEX", 3,
        w => { w.WriteArg(key); w.WriteArg(ttl); w.WriteArg(_serializer, value); },
        r => r.ReadOk());
    }

    public Invocation<bool> SetNX(string key, T value)
    {
      return invoke("SETNX", key, value, r => r.ReadBool());
    }

    public Invocation<int> SetRange(string key, int offset, T value)
    {
      return invoke("SETRANGE", 3,
        w => { w.WriteArg(key); w.WriteArg(offset); w.WriteArg(_serializer, value); },
        r => r.ReadInt());
    }

    public Invocation<int> SetRangeRaw(string key, int offset, byte[] value)
    {
      return invoke("SETRANGE", 3,
        w => { w.WriteArg(key); w.WriteArg(offset); w.WriteArg(value); },
        r => r.ReadInt());
    }

    public Invocation<int> SetRangeFrom(string key, int offset, Stream source, int count)
    {
      return invoke("SETRANGE", 3,
        w => { w.WriteArg(key); w.WriteArg(offset); w.WriteArg(source, count); },
        r => r.ReadInt());
    }

    public Invocation<int> Strlen(string key)
    {
      return invoke("STRLEN", key, r => r.ReadInt());
    }

    #endregion

    #region Hashes

    public Invocation<bool> HDel(string key, string field)
    {
      return invoke("HDEL", 2,
        w => { w.WriteArg(key); w.WriteArg(field); },
        r => r.ReadBool());
    }

    public Invocation<bool> HExists(string key, string field)
    {
      return invoke("HEXISTS", 2,
        w => { w.WriteArg(key); w.WriteArg(field); },
        r => r.ReadBool());
    }

    public Invocation<T> HGet(string key, string field)
    {
      return invoke("HGET", 2,
        w => { w.WriteArg(key); w.WriteArg(field); },
        _readObj);
    }

    public Invocation<byte[]> HGetRaw(string key, string field)
    {
      return invoke("HGET", 2,
        w => { w.WriteArg(key); w.WriteArg(field); },
        r => r.ReadRawBulk());
    }

    public Invocation<int> HGetTo(string key, string field, Stream target)
    {
      return invoke("HGET", 2,
        w => { w.WriteArg(key); w.WriteArg(field); },
        r => r.ReadStreamedBulk(target));
    }

    public Invocation<KeyValuePair<string, T>[]> HGetAll(string key)
    {
      return invoke("HGETALL", key, r => r.ReadSerializedKeyValues(_serializer));
    }

    public Invocation<long> HIncrBy(string key, string field, long amount)
    {
      return invoke("HINCRBY", 3,
        w => { w.WriteArg(key); w.WriteArg(field); w.WriteArg(amount); },
        r => r.ReadInt64());
    }

    public Invocation<string[]> HKeys(string key)
    {
      return invoke("HKEYS", key, r => r.ReadStrMultiBulk());
    }

    public Invocation<int> HLen(string key)
    {
      return invoke("HLEN", key, r => r.ReadInt());
    }

    public Invocation<T[]> HMGet(string key, params string[] fields)
    {
      return invoke("HMGET", fields.Length + 1,
        w => { w.WriteArg(key); Array.ForEach(fields, w.WriteArg); },
        _readObjs);
    }

    public Invocation<bool> HMSet(string key, IEnumerable<KeyValuePair<string, T>> mappings)
    {
      var arr = mappings.ToArray();
      return invoke("HMSET", arr.Length * 2 + 1,
        w =>
        {
          w.WriteArg(key);
          Array.ForEach(arr, kv =>
          {
            w.WriteArg(kv.Key);
            w.WriteArg(_serializer, kv.Value);
          });
        },
        r => r.ReadOk());
    }

    public Invocation<bool> HSet(string key, string field, T value)
    {
      return invoke("HSET", 3,
        w => { w.WriteArg(key); w.WriteArg(field); w.WriteArg(_serializer, value); },
        r => r.ReadBool());
    }

    public Invocation<bool> HSetRaw(string key, string field, byte[] data)
    {
      return invoke("HSET", 3,
        w => { w.WriteArg(key); w.WriteArg(field); w.WriteArg(data); },
        r => r.ReadBool());
    }

    public Invocation<bool> HSetFrom(string key, string field, Stream source, int count)
    {
      return invoke("HSET", 3,
        w => { w.WriteArg(key); w.WriteArg(field); w.WriteArg(source, count); },
        r => r.ReadBool());
    }

    public Invocation<bool> HSetNX(string key, string field, T value)
    {
      return invoke("HSETNX", 3,
        w => { w.WriteArg(key); w.WriteArg(field); w.WriteArg(_serializer, value); },
        r => r.ReadBool());
    }

    public Invocation<T[]> HVals(string key)
    {
      return invoke("HVALS", key, _readObjs);
    }

    #endregion

    #region Lists

    // a slight variation from Redis doc since params array must be last
    public Invocation<KeyValuePair<string, T>?> BLPop(int timeout, params string[] keys)
    {
      return invoke("BLPOP", keys.Length + 1,
        w => { w.WriteArg(timeout); Array.ForEach(keys, w.WriteArg); },
        r => r.ReadSerializedKeyValue(_serializer));
    }

    public Invocation<KeyValuePair<string, T>?> BRPop(int timeout, params string[] keys)
    {
      return invoke("BRPOP", keys.Length + 1,
        w => { w.WriteArg(timeout); Array.ForEach(keys, w.WriteArg); },
        r => r.ReadSerializedKeyValue(_serializer));
    }

    public Invocation<T> BRPopLPush(string src, string dest, int timeout)
    {
      return invoke("BRPOPLPUSH", 3,
        w => { w.WriteArg(src); w.WriteArg(dest); w.WriteArg(timeout); },
        _readObj);
    }

    public Invocation<T> LIndex(string key, int index)
    {
      return invoke("LINDEX", 2,
        w => { w.WriteArg(key); w.WriteArg(index); },
        _readObj);
    }

    // TODO: Implement LInsert with complex args
    public Invocation<int> LInsert(string key, T pivot, T value,
      bool afterPivot = false)
    {
      return invoke("LINSERT", 4,
        w =>
        {
          w.WriteArg(key);
          w.WriteArg(afterPivot ? "AFTER" : "BEFORE");
          w.WriteArg(_serializer, pivot);
          w.WriteArg(_serializer, value);
        },
        r => r.ReadInt());
    }

    public Invocation<int> LLen(string key)
    {
      return invoke("LLEN", key, r => r.ReadInt());
    }

    public Invocation<T> LPop(string key)
    {
      return invoke("LPOP", key, _readObj);
    }

    public Invocation<int> LPush(string key, T value)
    {
      return invoke("LPUSH", key, value, r => r.ReadInt());
    }

    public Invocation<int> LPushX(string key, T value)
    {
      return invoke("LPUSHX", key, value, r => r.ReadInt());
    }

    public Invocation<T[]> LRange(string key, int minIncl, int maxIncl)
    {
      return invoke("LRANGE", 3,
        w => { w.WriteArg(key); w.WriteArg(minIncl); w.WriteArg(maxIncl); },
        _readObjs);
    }

    public Invocation<int> LRem(string key, int count, T value)
    {
      return invoke("LREM", 3,
        w => { w.WriteArg(key); w.WriteArg(count); w.WriteArg(_serializer, value); },
        r => r.ReadInt());
    }

    public Invocation<bool> LSet(string key, int index, T value)
    {
      return invoke("LSET", 3,
        w => { w.WriteArg(key); w.WriteArg(index); w.WriteArg(_serializer, value); },
        r => r.ReadOk());
    }

    public Invocation<bool> LTrim(string key, int minIncl, int maxIncl)
    {
      return invoke("LTRIM", 3,
        w => { w.WriteArg(key); w.WriteArg(minIncl); w.WriteArg(maxIncl); },
        r => r.ReadOk());
    }

    public Invocation<T> RPop(string key)
    {
      return invoke("RPOP", key, _readObj);
    }

    public Invocation<T> RPopLPush(string srcKey, string destKey)
    {
      return invoke("RPOPLPUSH", 2,
        w => { w.WriteArg(srcKey); w.WriteArg(destKey); },
        _readObj);
    }

    public Invocation<int> RPush(string key, T value)
    {
      return invoke("RPUSH", key, value, r => r.ReadInt());
    }

    public Invocation<int> RPushX(string key, T value)
    {
      return invoke("RPUSHX", key, value, r => r.ReadInt());
    }

    #endregion

    #region Sets

    public Invocation<bool> SAdd(string key, T value)
    {
      return invoke("SADD", key, value, r => r.ReadBool());
    }

    public Invocation<int> SCard(string key)
    {
      return invoke("SCARD", key, r => r.ReadInt());
    }

    public Invocation<T[]> SDiff(params string[] keys)
    {
      return invoke("SDIFF", keys.Length,
        w => Array.ForEach(keys, w.WriteArg),
        _readObjs);
    }

    public Invocation<bool> SDiffStore(string destKey, params string[] keys)
    {
      return invoke("SDIFFSTORE", keys.Length + 1,
        w => { w.WriteArg(destKey); Array.ForEach(keys, w.WriteArg); },
        r => r.ReadOk());
    }

    public Invocation<T[]> SInter(params string[] keys)
    {
      return invoke("SINTER", keys.Length,
        w => Array.ForEach(keys, w.WriteArg),
        _readObjs);
    }

    public Invocation<bool> SInterStore(string destKey, params string[] keys)
    {
      return invoke("SINTERSTORE", keys.Length + 1,
        w => { w.WriteArg(destKey); Array.ForEach(keys, w.WriteArg); },
        r => r.ReadOk());
    }

    public Invocation<bool> SIsMember(string key, T value)
    {
      return invoke("SISMEMBER", key, value, r => r.ReadBool());
    }

    public Invocation<T[]> SMembers(string key)
    {
      return invoke("SMEMBERS", key, _readObjs);
    }

    public Invocation<bool> SMove(string srcKey, string destKey, T value)
    {
      return invoke("SMOVE", 3,
        w => { w.WriteArg(srcKey); w.WriteArg(destKey); w.WriteArg(_serializer, value); },
        r => r.ReadBool());
    }

    public Invocation<T> SPop(string key)
    {
      return invoke("SPOP", key, _readObj);
    }

    public Invocation<T> SRandMember(string key)
    {
      return invoke("SRANDMEMBER", key, _readObj);
    }

    public Invocation<bool> SRem(string key, T value)
    {
      return invoke("SREM", key, value, r => r.ReadBool());
    }

    public Invocation<T[]> SUnion(params string[] keys)
    {
      return invoke("SUNION", keys.Length,
        w => Array.ForEach(keys, w.WriteArg),
        _readObjs);
    }

    public Invocation<bool> SUnionStore(string destKey, params string[] keys)
    {
      return invoke("SUNIONSTORE", keys.Length + 1,
        w => { w.WriteArg(destKey); Array.ForEach(keys, w.WriteArg); },
        r => r.ReadOk());
    }

    #endregion

    #region Sorted Sets

    public Invocation<bool> ZAdd(string key, double score, T value)
    {
      return invoke("ZADD", 3,
        w => { w.WriteArg(key); w.WriteArg(score); w.WriteArg(_serializer, value); },
        r => r.ReadBool());
    }

    public Invocation<int> ZCard(string key)
    {
      return invoke("ZCARD", key, r => r.ReadInt());
    }

    public Invocation<int> ZCount(string key, double minIncl, double maxIncl)
    {
      return invoke("ZCOUNT", 3,
        w => { w.WriteArg(key); w.WriteArg(minIncl); w.WriteArg(maxIncl); },
        r => r.ReadInt());
    }

    public Invocation<double> ZIncrBy(string key, double amount, T value)
    {
      return invoke("ZINCRBY", 3,
        w => { w.WriteArg(key); w.WriteArg(amount); w.WriteArg(_serializer, value); },
        r => r.ReadDouble());
    }

    public Invocation<int> ZInterStore(string destKey, params string[] srcKeys)
    {
      return invoke("ZINTERSTORE", srcKeys.Length + 1,
        w => { w.WriteArg(destKey); Array.ForEach(srcKeys, w.WriteArg); },
        r => r.ReadInt());
    }

    public Invocation<T[]> ZRange(string key, int startRank, int endRank)
    {
      return invoke("ZRANGE", 3,
        w => { w.WriteArg(key); w.WriteArg(startRank); w.WriteArg(endRank); },
        _readObjs);
    }

    // TODO: ZRangeWithScores

    public Invocation<T[]> ZRangeByScore(string key, double minIncl, double maxIncl)
    {
      return invoke("ZRANGEBYSCORE", 3,
        w => { w.WriteArg(key); w.WriteArg(minIncl); w.WriteArg(maxIncl); },
        _readObjs);
    }

    public Invocation<int> ZRank(string key, T value)
    {
      return invoke("ZRANK", key, value, r => r.ReadInt());
    }

    public Invocation<bool> ZRem(string key, T value)
    {
      return invoke("ZREM", key, value, r => r.ReadBool());
    }

    public Invocation<int> ZRemRangeByRank(string key, int startRank, int endRank)
    {
      return invoke("ZRREMRANGEBYRANK", 3,
        w => { w.WriteArg(key); w.WriteArg(startRank); w.WriteArg(endRank); },
        r => r.ReadInt());
    }

    public Invocation<int> ZRemRangeByScore(string key, double minIncl, double maxIncl)
    {
      return invoke("ZREMRANGEBYSCORE", 3,
        w => { w.WriteArg(key); w.WriteArg(minIncl); w.WriteArg(maxIncl); },
        r => r.ReadInt());
    }

    public Invocation<T[]> ZRevRange(string key, int startRank, int endRank)
    {
      return invoke("ZREVRANGE", 3,
        w => { w.WriteArg(key); w.WriteArg(startRank); w.WriteArg(endRank); },
        _readObjs);
    }

    public Invocation<T[]> ZRevRangeByScore(string key, double minIncl, double maxIncl)
    {
      return invoke("ZREVRANGEBYSCORE", 3,
        w => { w.WriteArg(key); w.WriteArg(minIncl); w.WriteArg(maxIncl); },
        _readObjs);
    }

    public Invocation<int> ZRevRank(string key, T value)
    {
      return invoke("ZREVRANK", key, value, r => r.ReadInt());
    }

    public Invocation<double> ZScore(string key, T value)
    {
      return invoke("ZSCORE", key, value, r => r.ReadDouble());
    }

    public Invocation<int> ZUnionStore(string destKey, params string[] srcKeys)
    {
      return invoke("ZUNIONSTORE", srcKeys.Length + 1,
        w => { w.WriteArg(destKey); Array.ForEach(srcKeys, w.WriteArg); },
        r => r.ReadInt());
    }

    #endregion

    #region TODO: Pub/Sub

    // Could be done by using Observables
    // the tricky part will be how to implement unsubscribe?

    // public Invocation<IObservable<T>> PSubscribe(string key);
    // public Invocation<bool> Publish(string channel, T msg);
    // public Invocation<bool> PUnsubscribe(string key);
    // public Invocation<IObservable<T>> Subscribe(string key);
    // public Invocation<bool> Unsubscribe(string key);

    #endregion

  }
}
