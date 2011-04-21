
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Sider
{
  // see the redis commands reference for more info:
  // http://redis.io/commands
  public partial class RedisClient<T> : IRedisClient<T>
  {
    public IEnumerable<object> Pipeline(Action<IRedisClient<T>> pipelinedCalls)
    {
      if (_inTransaction)
        throw new InvalidOperationException(
          "Cannot call .Pipeline() while inside a MULTI/EXEC transaction.");

      // force-materialize the result to remove unwanted lazy-enumeration effect
      return executePipeline(pipelinedCalls).ToArray();
    }


    #region Server

    public bool BgRewriteAOF()
    {
      return execute(() =>
      {
        writeCmd("BGREWRITEAOF");
        return readStatus("Background append only file rewriting started");
      });
    }

    public bool BgSave()
    {
      return execute(() =>
      {
        writeCmd("BGSAVE");
        return readStatus("Background saving started");
      });
    }

    public KeyValuePair<string, string>[] ConfigGet(string param)
    {
      return execute(() =>
      {
        writeStrCmd("CONFIG", "GET", param);
        return readStringKeyValues();
      });
    }

    public bool ConfigSet(string param, string value)
    {
      return execute(() =>
      {
        writeStrCmd("CONFIG", "SET", param, value);
        return readOk();
      });
    }

    public bool ConfigResetStat()
    {
      return execute(() =>
      {
        writeCmd("CONFIG", "RESETSTAT");
        return readOk();
      });
    }

    public int DbSize()
    {
      return execute(() =>
      {
        writeCmd("DBSIZE");
        return readInt();
      });
    }

    public string DebugObject(string key)
    {
      return execute(() =>
      {
        writeStrCmd("DEBUG", "OBJECT", key);
        return readStatus();
      });
    }

    public void DebugSegfault()
    {
      execute(() =>
      {
        writeCmd("DEBUG", "SEGFAULT");
        Dispose();
      });
    }

    public bool FlushAll()
    {
      return execute(() =>
      {
        writeCmd("FLUSHALL");
        return readBool();
      });
    }

    public bool FlushDb()
    {
      return execute(() =>
      {
        writeCmd("FLUSHDB");
        return readOk();
      });
    }

    public DateTime LastSave()
    {
      return execute(() =>
      {
        writeCmd("LASTSAVE");
        return readCore(ResponseType.Integer, r =>
          parseDateTime(r.ReadNumberLine64()));
      });
    }

    public bool Save()
    {
      return execute(() =>
      {
        writeCmd("SAVE");
        return readOk();
      });
    }

    public void Shutdown()
    {
      execute(() =>
      {
        writeCmd("SHUTDOWN");
        // TODO: Docs says error is possible but how to produce it?
        Dispose();
      });
    }

    // TODO: MONITOR using IObservable (after PUB/SUB)
    // public IObservable<string> Monitor()

    public IEnumerable<KeyValuePair<string, string>> Info(string section = null)
    {
      return execute(() =>
      {
        writeCmd("INFO");
        var rawResult = readStrBulk()
          .Split(new[] { '\r', '\n', ':' }, StringSplitOptions.RemoveEmptyEntries)
          .ToArray();

        var result = new KeyValuePair<string, string>[rawResult.Length / 2];
        var resultIdx = 0;
        for (var i = 0; i < rawResult.Length; i += 2)
          result[resultIdx++] = new KeyValuePair<string, string>(
            rawResult[i], rawResult[i + 1]);

        return result;
      });
    }

    public bool SlaveOf(string host, int port)
    {
      return execute(() =>
      {
        writeStrCmd("SLAVEOF", host, port.ToString());
        return readOk();
      });
    }

    // NOTE: SYNC ... ?

    #endregion

    #region Connection

    public bool Auth(string password)
    {
      return execute(() =>
      {
        writeCmd("AUTH", password);
        return readOk();
      });
    }

    public string Echo(string msg)
    {
      return execute(() =>
      {
        writeCmd("ECHO", msg);
        return readStrBulk();
      });
    }

    // TODO: `T Echo(T msg)` overload ?

    public bool Ping()
    {
      return execute(() =>
      {
        writeCmd("PING");
        return readStatus("PONG");
      });
    }

    public void Quit()
    {
      execute(() =>
      {
        writeCmd("QUIT");
        readOk();
        Dispose();
      });
    }

    public bool Select(int dbIndex)
    {
      return execute(() =>
      {
        writeCmd("SELECT", dbIndex.ToString());
        return readOk();
      });
    }

    #endregion

    #region Transactions

    public bool Multi()
    {
      if (_isPipelining)
        throw new InvalidOperationException(
          "Cannot call .Multi() while inside a .Pipeline() calls.\r\n" +
          "MULTI/EXEC calls are automatically pipelined.");

      return execute(() =>
      {
        writeCmd("MULTI");
        var result = readOk();

        beginMultiExec();
        return result;
      });
    }

    public bool Discard()
    {
      return execute(() =>
      {
        writeCmd("DISCARD");
        endMultiExec();
        return readOk();
      });
    }

    public IEnumerable<object> Exec()
    {
      return execute(() =>
      {
        writeCmd("EXEC");
        endMultiExec();

        var count = readCore(ResponseType.MultiBulk, r => r.ReadNumberLine());
        SAssert.IsTrue(count == _readsQueue.Count,
          () => new ResponseException("EXEC returned wrong number of bulks to read."));

        return executeQueuedReads();
      });
    }

    public bool Watch(params string[] keys)
    {
      return execute(() =>
      {
        writeCmd("WATCH", keys);
        return readOk();
      });
    }

    public bool Unwatch()
    {
      return execute(() =>
      {
        writeCmd("UNWATCH");
        return readOk();
      });
    }

    #endregion

    #region Keys

    public int Del(params string[] keys)
    {
      return execute(() =>
      {
        writeCmd("DEL", keys);
        return readInt();
      });
    }

    public bool Exists(string key)
    {
      return execute(() =>
      {
        writeCmd("EXISTS", key);
        return readBool();
      });
    }

    public bool Expire(string key, TimeSpan span)
    {
      return execute(() =>
      {
        writeStrCmd("EXPIRE", key, formatTimeSpan(span));
        return readBool();
      });
    }

    public bool ExpireAt(string key, DateTime time)
    {
      return execute(() =>
      {
        writeStrCmd("EXPIREAT", key, formatDateTime(time));
        return readBool();
      });
    }

    public string[] Keys(string pattern)
    {
      return execute(() =>
      {
        writeCmd("KEYS", pattern);
        return readStrMultiBulk();
      });
    }

    public bool Move(string key, int dbIndex)
    {
      return execute(() =>
      {
        writeStrCmd("MOVE", key, dbIndex.ToString());
        return readBool();
      });
    }

    public bool Persist(string key)
    {
      return execute(() =>
      {
        writeCmd("PERSIST", key);
        return readBool();
      });
    }

    public string RandomKey()
    {
      return execute(() =>
      {
        writeCmd("RANDOMKEY");
        return readStrBulk();
      });
    }

    public bool Rename(string oldKey, string newKey)
    {
      return execute(() =>
      {
        writeStrCmd("RENAME", oldKey, newKey);
        return readOk();
      });
    }

    public bool RenameNX(string oldKey, string newKey)
    {
      return execute(() =>
      {
        writeStrCmd("RENAMENX", oldKey, newKey);
        return readBool();
      });
    }

    // SORT key [BY pattern] [LIMIT offset count] [GET pattern [GET pattern ...]]
    //     [ASC|DESC] [ALPHA] [STORE destination]
    public T[] Sort(string key, string byPattern = null,
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

      return execute(() =>
      {
        writeCmd("SORT", items.ToArray());
        return readMultiBulk();
      });
    }

    public TimeSpan TTL(string key)
    {
      return execute(() =>
      {
        writeCmd("TTL", key);
        return readCore(ResponseType.Integer,
          r => TimeSpan.FromSeconds(r.ReadNumberLine64()));
      });
    }

    public RedisType Type(string key)
    {
      return execute(() =>
      {
        writeCmd("TYPE", key);
        return readCore(ResponseType.SingleLine, r =>
          RedisTypes.Parse(r.ReadStatusLine()));
      });
    }

    #endregion

    #region Strings

    public int Append(string key, T value)
    {
      return execute(() =>
      {
        writeCmd("APPEND", key, value);
        return readInt();
      });
    }

    public long Decr(string key)
    {
      return execute(() =>
      {
        writeCmd("DECR", key);
        return readInt64();
      });
    }

    public long DecrBy(string key, long value)
    {
      return execute(() =>
      {
        writeStrCmd("DECRBY", key, value.ToString());
        return readInt64();
      });
    }

    public T Get(string key)
    {
      return execute(() =>
      {
        writeCmd("GET", key);
        return readBulk();
      });
    }

    public byte[] GetRaw(string key)
    {
      return execute(() =>
      {
        writeCmd("GET", key);
        return readBulkRaw();
      });
    }

    public int GetTo(string key, Stream target)
    {
      return execute(() =>
      {
        writeCmd("GET", key);
        return readBulkTo(target);
      });
    }

    public int GetBit(string key, int offset)
    {
      return execute(() =>
      {
        writeStrCmd("GETBIT", key, offset.ToString());
        return readInt();
      });
    }

    public T GetRange(string key, int start, int end)
    {
      return execute(() =>
      {
        writeStrCmd("GETRANGE", key, start.ToString(), end.ToString());
        return readBulk();
      });
    }

    public byte[] GetRangeRaw(string key, int start, int end)
    {
      return execute(() =>
      {
        writeStrCmd("GETRANGE", key, start.ToString(), end.ToString());
        return readBulkRaw();
      });
    }

    public int GetRangeTo(string key, int start, int end, Stream source)
    {
      return execute(() =>
      {
        writeStrCmd("GETRANGE", key, start.ToString(), end.ToString());
        return readBulkTo(source);
      });
    }

    public T GetSet(string key, T value)
    {
      return execute(() =>
      {
        writeCmd("GETSET", key, value);
        return readBulk();
      });
    }

    public long Incr(string key)
    {
      return execute(() =>
      {
        writeCmd("INCR", key);
        return readInt64();
      });
    }

    public long IncrBy(string key, long value)
    {
      return execute(() =>
      {
        writeStrCmd("INCRBY", key, value.ToString());
        return readInt64();
      });
    }

    public T[] MGet(params string[] keys)
    {
      return execute(() =>
      {
        writeCmd("MGET", keys);
        return readMultiBulk();
      });
    }

    public bool MSet(IEnumerable<KeyValuePair<string, T>> mappings)
    {
      return execute(() =>
      {
        writeCmd("MSET", mappings.ToArray());
        return readOk();
      });
    }

    public bool MSetNX(IEnumerable<KeyValuePair<string, T>> mappings)
    {
      return execute(() =>
      {
        writeCmd("MSETNX", mappings.ToArray());
        return readBool();
      });
    }

    public bool Set(string key, T value)
    {
      return execute(() =>
      {
        writeCmd("SET", key, value);
        return readOk();
      });
    }

    public bool SetRaw(string key, byte[] raw)
    {
      return execute(() =>
      {
        writeCmd("SET", key, raw);
        return readOk();
      });
    }

    public bool SetFrom(string key, Stream source, int count)
    {
      return execute(() =>
      {
        writeCmd("SET", key, source, count);
        return readOk();
      });
    }

    public int SetBit(string key, int offset, int value)
    {
      return execute(() =>
      {
        writeStrCmd("SETBIT", key, offset.ToString(), value.ToString());
        return readInt();
      });
    }

    public bool SetEX(string key, TimeSpan ttl, T value)
    {
      return execute(() =>
      {
        writeCmd("SETEX", key, formatTimeSpan(ttl), value);
        return readOk();
      });
    }

    public bool SetNX(string key, T value)
    {
      return execute(() =>
      {
        writeCmd("SETNX", key, value);
        return readBool();
      });
    }

    public int SetRange(string key, int offset, T value)
    {
      return execute(() =>
      {
        writeCmd("SETRANGE", key, offset.ToString(), value);
        return readInt();
      });
    }

    public int SetRangeRaw(string key, int offset, byte[] value)
    {
      return execute(() =>
      {
        writeCmd("SETRANGE", key, offset.ToString(), value);
        return readInt();
      });
    }

    public int SetRangeFrom(string key, int offset, Stream source, int count)
    {
      return execute(() =>
      {
        writeCmd("SETRANGE", key, offset.ToString(), source, count);
        return readInt();
      });
    }

    public int Strlen(string key)
    {
      return execute(() =>
      {
        writeCmd("STRLEN", key);
        return readInt();
      });
    }

    #endregion

    #region Hashes

    public bool HDel(string key, string field)
    {
      return execute(() =>
      {
        writeStrCmd("HDEL", key, field);
        return readBool();
      });
    }

    public bool HExists(string key, string field)
    {
      return execute(() =>
      {
        writeStrCmd("HEXISTS", key, field);
        return readBool();
      });
    }

    public T HGet(string key, string field)
    {
      return execute(() =>
      {
        writeStrCmd("HGET", key, field);
        return readBulk();
      });
    }

    public byte[] HGetRaw(string key, string field)
    {
      return execute(() =>
      {
        writeStrCmd("HGET", key, field);
        return readBulkRaw();
      });
    }

    public int HGetTo(string key, string field, Stream target)
    {
      return execute(() =>
      {
        writeStrCmd("HGET", key, field);
        return readCore(ResponseType.Bulk, r =>
        {
          var length = r.ReadNumberLine();
          if (length > -1)
            r.ReadBulkTo(target, length);

          return length;
        });
      });
    }

    public IEnumerable<KeyValuePair<string, T>> HGetAll(string key)
    {
      return execute(() =>
      {
        writeCmd("HGETALL", key);
        return readKeyValues();
      });
    }

    public long HIncrBy(string key, string field, long amount)
    {
      return execute(() =>
      {
        writeStrCmd("HINCRBY", key, field, amount.ToString());
        return readInt64();
      });
    }

    public string[] HKeys(string key)
    {
      return execute(() =>
      {
        writeCmd("HKEYS", key);
        return readStrMultiBulk();
      });
    }

    public int HLen(string key)
    {
      return execute(() =>
      {
        writeCmd("HLEN", key);
        return readInt();
      });
    }

    public T[] HMGet(string key, params string[] fields)
    {
      return execute(() =>
      {
        writeCmd("HMGET", key, fields);
        return readMultiBulk();
      });
    }

    public bool HMSet(string key, IEnumerable<KeyValuePair<string, T>> mappings)
    {
      return execute(() =>
      {
        writeCmd("HMSET", key, mappings.ToArray());
        return readOk();
      });
    }

    public bool HSet(string key, string field, T value)
    {
      return execute(() =>
      {
        writeCmd("HSET", key, field, value);
        return readBool();
      });
    }

    public bool HSetRaw(string key, string field, byte[] data)
    {
      return execute(() =>
      {
        writeCmd("HSET", key, field, data);
        return readBool();
      });
    }

    public bool HSetFrom(string key, string field, Stream source, int count)
    {
      return execute(() =>
      {
        writeCmd("HSET", key, field, source, count);
        return readBool();
      });
    }

    public bool HSetNX(string key, string field, T value)
    {
      return execute(() =>
      {
        writeCmd("HSETNX", key, field, value);
        return readBool();
      });
    }

    public T[] HVals(string key)
    {
      return execute(() =>
      {
        writeCmd("HVALS", key);
        return readMultiBulk();
      });
    }

    #endregion

    #region Lists

    // a slight variation from Redis doc since params array must be last
    public KeyValuePair<string, T>? BLPop(int timeout, params string[] keys)
    {
      if (_inTransaction)
        throw new InvalidOperationException(
          "BLPOP cannot be issued while inside a MULTI/EXEC transaction.");

      return execute(() =>
      {
        writeCmd("BLPOP", keys, timeout.ToString());
        return readKeyValue();
      });
    }

    public KeyValuePair<string, T>? BRPop(int timeout, params string[] keys)
    {
      if (_inTransaction)
        throw new InvalidOperationException(
          "BRPOP cannot be issued while inside a MULTI/EXEC transaction.");

      return execute(() =>
      {
        writeCmd("BRPOP", keys, timeout.ToString());
        return readKeyValue();
      });
    }

    public T BRPopLPush(string src, string dest, int timeout)
    {
      if (_inTransaction)
        throw new InvalidOperationException(
          "BRPOPLPUSH cannot be issued while inside a MULTI/EXEC transaction.");

      return execute(() =>
      {
        writeStrCmd("BRPOPLPUSH", src, dest, timeout.ToString());

        // NOTE: There is inconsistency in redis protocol v < 2.2 which
        //   returns a Multi-Bulk nil when the timeout occurs instead of a
        //   bulk-nil, this should be fixed in 2.2 so I'm leaving this as-is
        return readBulk();
      });
    }

    public T LIndex(string key, int index)
    {
      return execute(() =>
      {
        writeStrCmd("LINDEX", key, index.ToString());
        return readBulk();
      });
    }

    // TODO: Implement LInsert with complex args

    public int LLen(string key)
    {
      return execute(() =>
      {
        writeCmd("LLEN", key);
        return readInt();
      });
    }

    public T LPop(string key)
    {
      return execute(() =>
      {
        writeCmd("LPOP", key);
        return readBulk();
      });
    }

    public int LPush(string key, T value)
    {
      return execute(() =>
      {
        writeCmd("LPUSH", key, value);
        return readInt();
      });
    }

    public int LPushX(string key, T value)
    {
      return execute(() =>
      {
        writeCmd("LPUSHX", key, value);
        return readInt();
      });
    }

    public T[] LRange(string key, int minIncl, int maxIncl)
    {
      return execute(() =>
      {
        writeStrCmd("LRANGE", key, minIncl.ToString(), maxIncl.ToString());
        return readMultiBulk();
      });
    }

    public int LRem(string key, int count, T value)
    {
      return execute(() =>
      {
        writeCmd("LREM", key, count.ToString(), value);
        return readInt();
      });
    }

    public bool LSet(string key, int index, T value)
    {
      return execute(() =>
      {
        writeCmd("LSET", key, index.ToString(), value);
        return readOk();
      });
    }

    public bool LTrim(string key, int minIncl, int maxIncl)
    {
      return execute(() =>
      {
        writeStrCmd("LTRIM", key, minIncl.ToString(), maxIncl.ToString());
        return readOk();
      });
    }

    public T RPop(string key)
    {
      return execute(() =>
      {
        writeCmd("RPOP", key);
        return readBulk();
      });
    }

    public T RPopLPush(string srcKey, string destKey)
    {
      return execute(() =>
      {
        writeStrCmd("RPOPLPUSH", srcKey, destKey);
        return readBulk();
      });
    }

    public int RPush(string key, T value)
    {
      return execute(() =>
      {
        writeCmd("RPUSH", key, value);
        return readInt();
      });
    }

    public int RPushX(string key, T value)
    {
      return execute(() =>
      {
        writeCmd("RPUSHX", key, value);
        return readInt();
      });
    }

    #endregion

    #region Sets

    public bool SAdd(string key, T value)
    {
      return execute(() =>
      {
        writeCmd("SADD", key, value);
        return readBool();
      });
    }

    public int SCard(string key)
    {
      return execute(() =>
      {
        writeCmd("SCARD", key);
        return readInt();
      });
    }

    public T[] SDiff(params string[] keys)
    {
      return execute(() =>
      {
        writeCmd("SDIFF", keys);
        return readMultiBulk();
      });
    }

    public bool SDiffStore(string destKey, params string[] keys)
    {
      return execute(() =>
      {
        writeCmd("SDIFFSTORE", destKey, keys);
        return readOk();
      });
    }

    public T[] SInter(params string[] keys)
    {
      return execute(() =>
      {
        writeCmd("SINTER", keys);
        return readMultiBulk();
      });
    }

    public bool SInterStore(string destKey, params string[] keys)
    {
      return execute(() =>
      {
        writeCmd("SINTERSTORE", destKey, keys);
        return readOk();
      });
    }

    public bool SIsMember(string key, T value)
    {
      return execute(() =>
      {
        writeCmd("SISMEMBER", key, value);
        return readBool();
      });
    }

    public T[] SMembers(string key)
    {
      return execute(() =>
      {
        writeCmd("SMEMBERS", key);
        return readMultiBulk();
      });
    }

    public bool SMove(string srcKey, string destKey, T value)
    {
      return execute(() =>
      {
        writeCmd("SMOVE", srcKey, destKey, value);
        return readBool();
      });
    }

    public T SPop(string key)
    {
      return execute(() =>
      {
        writeCmd("SPOP", key);
        return readBulk();
      });
    }

    public T SRandMember(string key)
    {
      return execute(() =>
      {
        writeCmd("SRANDMEMBER", key);
        return readBulk();
      });
    }

    public bool SRem(string key, T value)
    {
      return execute(() =>
      {
        writeCmd("SREM", key, value);
        return readBool();
      });
    }

    public T[] SUnion(params string[] keys)
    {
      return execute(() =>
      {
        writeCmd("SUNION", keys);
        return readMultiBulk();
      });
    }

    public bool SUnionStore(string destKey, params string[] keys)
    {
      return execute(() =>
      {
        writeCmd("SUNIONSTORE", destKey, keys);
        return readOk();
      });
    }

    #endregion

    #region Sorted Sets

    public bool ZAdd(string key, double score, T value)
    {
      return execute(() =>
      {
        writeCmd("ZADD", key, formatDbl(score), value);
        return readBool();
      });
    }

    public int ZCard(string key)
    {
      writeCmd("ZCARD", key);
      return readInt();
    }

    public int ZCount(string key, double minIncl, double maxIncl)
    {
      return execute(() =>
      {
        writeStrCmd("ZCOUNT", key, formatDbl(minIncl), formatDbl(maxIncl));
        return readInt();
      });
    }

    public double ZIncrBy(string key, double amount, T value)
    {
      return execute(() =>
      {
        writeCmd("ZINCRBY", key, formatDbl(amount), value);
        return readDouble();
      });
    }

    public int ZInterStore(string destKey, params string[] srcKeys)
    {
      return execute(() =>
      {
        writeCmd("ZINTERSTORE", destKey, srcKeys.Length.ToString(), srcKeys);
        return readInt();
      });
    }

    public T[] ZRange(string key, int startRank, int endRank)
    {
      return execute(() =>
      {
        writeStrCmd("ZRANGE", key, startRank.ToString(), endRank.ToString());
        return readMultiBulk();
      });
    }

    public T[] ZRangeByScore(string key, double minIncl, double maxIncl)
    {
      return execute(() =>
      {
        writeStrCmd("ZRANGEBYSCORE", key, formatDbl(minIncl), formatDbl(maxIncl));
        return readMultiBulk();
      });
    }

    public int ZRank(string key, T value)
    {
      return execute(() =>
      {
        writeCmd("ZRANK", key, value);
        return readInt();
      });
    }

    public bool ZRem(string key, T value)
    {
      return execute(() =>
      {
        writeCmd("ZREM", key, value);
        return readBool();
      });
    }

    public int ZRemRangeByRank(string key, int startRank, int endRank)
    {
      return execute(() =>
      {
        writeStrCmd("ZREMRANGEBYRANK", key, startRank.ToString(), endRank.ToString());
        return readInt();
      });
    }

    public int ZRemRangeByScore(string key, double minIncl, double maxIncl)
    {
      return execute(() =>
      {
        writeStrCmd("ZREMRANGEBYSCORE", key, formatDbl(minIncl), formatDbl(maxIncl));
        return readInt();
      });
    }

    public T[] ZRevRange(string key, int startRank, int endRank)
    {
      return execute(() =>
      {
        writeStrCmd("ZREVRANGE", key, startRank.ToString(), endRank.ToString());
        return readMultiBulk();
      });
    }

    public T[] ZRevRangeByScore(string key, double minIncl, double maxIncl)
    {
      return execute(() =>
      {
        writeStrCmd("ZREVRANGE", key, formatDbl(minIncl), formatDbl(maxIncl));
        return readMultiBulk();
      });
    }

    public int ZRevRank(string key, T value)
    {
      return execute(() =>
      {
        writeCmd("ZREVRANK", key, value);
        return readInt();
      });
    }

    public double ZScore(string key, T value)
    {
      return execute(() =>
      {
        writeCmd("ZSCORE", key);
        return readDouble();
      });
    }

    public int ZUnionStore(string destKey, params string[] srcKeys)
    {
      return execute(() =>
      {
        writeCmd("ZUNIONSTORE", destKey, srcKeys.Length.ToString(), srcKeys);
        return readInt();
      });
    }

    #endregion

    #region TODO: Pub/Sub

    // Could be done by using Observables
    // the tricky part will be how to implement unsubscribe?

    // public IObservable<T> PSubscribe(string key);
    // public bool Publish(string channel, T msg);
    // public bool PUnsubscribe(string key);
    // public IObservable<T> Subscribe(string key);
    // public bool Unsubscribe(string key);

    #endregion
  }
}
