
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Sider.Executors;

namespace Sider
{
  // REF: http://redis.io/commands
  public partial class RedisClient<T> : IRedisClient<T>
  {
    // TODO: IocpExecutor?
    public IEnumerable<object> Pipeline(Action<IRedisClient<T>> pipelinedCalls)
    {
      // execute in pipelined mode
      var result = SwitchExecutor<PipelinedExecutor>()
        .ExecuteRange(pipelinedCalls);

      // end pipelined mode
      SwitchExecutor<ImmediateExecutor>();
      return result;
    }

    public TReturn Custom<TReturn>(string command,
      Action<ProtocolWriter> writeAction,
      Func<ProtocolReader, TReturn> readAction)
    {
      return invoke(Invocation.New(command, writeAction, readAction));
    }


    #region Server

    public bool BgRewriteAof()
    {
      return invoke("BGREWRITEAOF",
        r => r.ReadStatus("Background append only file rewriting started"));
    }

    public bool BgSave()
    {
      return invoke("BGSAVE", r => r.ReadStatus("Background saving started"));
    }

    public KeyValuePair<string, string>[] ConfigGet(string param)
    {
      return invoke("CONFIG", 2,
        w => { w.WriteArg("GET"); w.WriteArg(param); },
        r => r.ReadStrKeyValues());
    }

    public bool ConfigSet(string param, string value)
    {
      return invoke("CONFIG", 3,
        w => { w.WriteArg("SET"); w.WriteArg(param); w.WriteArg(value); },
        r => r.ReadOk());
    }

    public bool ConfigResetStat()
    {
      return invoke("CONFIG", "RESETSTAT", r => r.ReadOk());
    }

    public int DbSize()
    {
      return invoke("DBSIZE", r => r.ReadInt());
    }

    public string DebugObject(string key)
    {
      return invoke("DEBUG", 2,
        w => { w.WriteArg("OBJECT"); w.WriteArg(key); },
        r => r.ReadStatus());
    }

    public void DebugSegfault()
    {
      invoke("DEBUG", "SEGFAULT", r => (object)null);
    }

    public bool FlushAll()
    {
      return invoke("FLUSHALL", r => r.ReadOk());
    }

    public bool FlushDb()
    {
      return invoke("FLUSHDB", r => r.ReadOk());
    }

    public DateTime LastSave()
    {
      return invoke("LASTSAVE", r => r.ReadDateTime());
    }

    public IObservable<string> Monitor()
    {
      invoke("MONITOR", r => r.ReadOk());

      return SwitchExecutor<MonitorExecutor>()
        .BuildObservable();
    }

    public bool Save()
    {
      return invoke("SAVE", r => r.ReadOk());
    }

    public void Shutdown()
    {
      invoke("SHUTDOWN");
    }

    // TODO: section support
    public KeyValuePair<string, string>[] Info()
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

    public bool SlaveOf(string host, int port)
    {
      return invoke("SLAVEOF", 2,
        w => { w.WriteArg(host); w.WriteArg(port); },
        r => r.ReadOk());
    }

    #endregion

    #region Connection

    public bool Auth(string password)
    {
      return invoke("AUTH", password, r => r.ReadOk());
    }

    public string Echo(string msg)
    {
      return invoke("ECHO", msg, r => r.ReadStrBulk());
    }

    public T Echo(T msg)
    {
      return invoke("ECHO", 1,
        w => w.WriteArg(_serializer, msg),
        _readObj);
    }

    public bool Ping()
    {
      return invoke("PING", 0,
        w => { },
        r => r.ReadStatus("PONG"));
    }

    public void Quit()
    {
      invoke("QUIT", r => r.ReadOk());
      Dispose();
    }

    public bool Select(int dbIndex)
    {
      return invoke("SELECT", 1,
        w => w.WriteArg(dbIndex),
        r => r.ReadOk());
    }

    #endregion

    #region Transactions

    public bool Multi()
    {
      var result = invoke("MULTI", r => r.ReadOk());

      SwitchExecutor<TransactedExecutor>();
      return result;
    }

    public bool Discard()
    {
      var te = Executor as TransactedExecutor;
      if (te != null) {
        te.Discard();
        SwitchExecutor<ImmediateExecutor>();
      }

      return invoke("DISCARD", r => r.ReadOk());
    }

    public IEnumerable<object> Exec()
    {
      var te = Executor as TransactedExecutor;
      if (te == null) {
        // ReadStatus so error will be thrown, if any
        invoke("EXEC", r => r.ReadStatus());
        return null;
      }

      // executor automatically issue EXEC
      var result = te.Finish();

      // restore execution mode
      SwitchExecutor<ImmediateExecutor>();
      return result;
    }

    public bool Watch(params string[] keys)
    {
      return invoke("WATCH", keys.Length,
        w => Array.ForEach(keys, w.WriteArg),
        r => r.ReadOk());
    }

    public bool Unwatch()
    {
      return invoke("UNWATCH", r => r.ReadOk());
    }

    #endregion

    #region Keys

    public int Del(params string[] keys)
    {
      return invoke("DEL", keys.Length,
        w => Array.ForEach(keys, w.WriteArg),
        r => r.ReadInt());
    }

    public bool Exists(string key)
    {
      return invoke("EXISTS", key, r => r.ReadBool());
    }

    public bool Expire(string key, TimeSpan span)
    {
      return invoke("EXPIRE", 2,
        w => { w.WriteArg(key); w.WriteArg(span); },
        r => r.ReadBool());
    }

    public bool ExpireAt(string key, DateTime time)
    {
      return invoke("EXPIREAT", 2,
        w => { w.WriteArg(key); w.WriteArg(time); },
        r => r.ReadBool());
    }

    public string[] Keys(string pattern)
    {
      return invoke("KEYS", pattern, r => r.ReadStrMultiBulk());
    }

    public bool Move(string key, int dbIndex)
    {
      return invoke("MOVE", 2,
        w => { w.WriteArg(key); w.WriteArg(dbIndex); },
        r => r.ReadBool());
    }

    public bool Persist(string key)
    {
      return invoke("PERSIST", r => r.ReadBool());
    }

    public string RandomKey()
    {
      return invoke("RANDOMKEY", r => r.ReadStrBulk());
    }

    public bool Rename(string oldKey, string newKey)
    {
      return invoke("RENAME", 2,
        w => { w.WriteArg(oldKey); w.WriteArg(newKey); },
        r => r.ReadOk());
    }

    public bool RenameNX(string oldKey, string newKey)
    {
      return invoke("RENAMENX", 2,
        w => { w.WriteArg(oldKey); w.WriteArg(newKey); },
        r => r.ReadBool());
    }

    // SORT key [BY pattern] [LIMIT offset count] [GET pattern [GET pattern ...]]
    //     [ASC|DESC] [ALPHA] [STORE destination]
    public T[] Sort(string key, string byPattern = null,
      int? limitOffset = null, int? limitCount = null,
      string[] getPattern = null, bool descending = false,
      bool alpha = false, string store = null)
    {
      var items = new LinkedList<string>();
      items.AddLast(key);

      if (!string.IsNullOrEmpty(byPattern)) {
        items.AddLast("BY");
        items.AddLast(byPattern);
      }

      if (limitOffset.HasValue || limitCount.HasValue) {
        items.AddLast("LIMIT");
        items.AddLast(limitOffset.GetValueOrDefault(0).ToString());
        items.AddLast(limitCount.GetValueOrDefault(int.MaxValue).ToString());
      }

      if (getPattern != null) {
        foreach (var pattern in getPattern) {
          items.AddLast("GET");
          items.AddLast(pattern);
        }
      }

      if (descending) items.AddLast("DESC");
      if (alpha) items.AddLast("ALPHA");

      if (!string.IsNullOrEmpty(store)) {
        items.AddLast("STORE");
        items.AddLast(store);
      }

      return invoke("SORT", items.Count,
        w => { foreach (var item in items) w.WriteArg(item); },
        _readObjs);
    }

    public TimeSpan TTL(string key)
    {
      return invoke("TTL", key, r => r.ReadTimeSpan());
    }

    public RedisType Type(string key)
    {
      return invoke("TYPE", key, r => RedisTypes.Parse(r.ReadStatus()));
    }

    #endregion

    #region Strings

    public int Append(string key, T value)
    {
      return invoke("APPEND", key, value, r => r.ReadInt());
    }

    public long Decr(string key)
    {
      return invoke("DECR", key, r => r.ReadInt64());
    }

    public long DecrBy(string key, long value)
    {
      return invoke("DECRBY", 2,
        w => { w.WriteArg(key); w.WriteArg(value); },
        r => r.ReadInt64());
    }

    public T Get(string key)
    {
      return invoke("GET", key, _readObj);
    }

    public byte[] GetRaw(string key)
    {
      return invoke("GET", key, r => r.ReadRawBulk());
    }

    public int GetTo(string key, Stream target)
    {
      return invoke("GET", key, r => r.ReadStreamedBulk(target));
    }

    public int GetBit(string key, int offset)
    {
      return invoke("GETBIT", 2,
        w => { w.WriteArg(key); w.WriteArg(offset); },
        r => r.ReadInt());
    }

    public T GetRange(string key, int start, int end)
    {
      return invoke("GETRANGE", 3,
        w => { w.WriteArg(key); w.WriteArg(start); w.WriteArg(end); },
        _readObj);
    }

    public byte[] GetRangeRaw(string key, int start, int end)
    {
      return invoke("GETRANGE", 3,
        w => { w.WriteArg(key); w.WriteArg(start); w.WriteArg(end); },
        r => r.ReadRawBulk());
    }

    public int GetRangeTo(string key, int start, int end, Stream target)
    {
      return invoke("GETRANGE", 3,
        w => { w.WriteArg(key); w.WriteArg(start); w.WriteArg(end); },
        r => r.ReadStreamedBulk(target));
    }

    public T GetSet(string key, T value)
    {
      return invoke("GETSET", key, value, _readObj);
    }

    public long Incr(string key)
    {
      return invoke("INCR", key, r => r.ReadInt64());
    }

    public long IncrBy(string key, long value)
    {
      return invoke("INCRBY", 2,
        w => { w.WriteArg(key); w.WriteArg(value); },
        r => r.ReadInt64());
    }

    public T[] MGet(params string[] keys)
    {
      return invoke("MGET", keys.Length,
        w => Array.ForEach(keys, w.WriteArg),
        _readObjs);
    }

    public bool MSet(IEnumerable<KeyValuePair<string, T>> mappings)
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

    public bool MSetNX(IEnumerable<KeyValuePair<string, T>> mappings)
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

    public bool Set(string key, T value)
    {
      return invoke("SET", key, value, r => r.ReadOk());
    }

    public bool SetRaw(string key, byte[] raw)
    {
      return invoke("SET", 2,
        w => { w.WriteArg(key); w.WriteArg(raw); },
        r => r.ReadOk());
    }

    public bool SetFrom(string key, Stream source, int count)
    {
      return invoke("SET", 2,
        w => { w.WriteArg(key); w.WriteArg(source, count); },
        r => r.ReadOk());
    }

    public int SetBit(string key, int offset, int value)
    {
      return invoke("SETBIT", 3,
        w => { w.WriteArg(key); w.WriteArg(offset); w.WriteArg(value); },
        r => r.ReadInt());
    }

    public bool SetEX(string key, TimeSpan ttl, T value)
    {
      return invoke("SETEX", 3,
        w => { w.WriteArg(key); w.WriteArg(ttl); w.WriteArg(_serializer, value); },
        r => r.ReadOk());
    }

    public bool SetNX(string key, T value)
    {
      return invoke("SETNX", key, value, r => r.ReadBool());
    }

    public int SetRange(string key, int offset, T value)
    {
      return invoke("SETRANGE", 3,
        w => { w.WriteArg(key); w.WriteArg(offset); w.WriteArg(_serializer, value); },
        r => r.ReadInt());
    }

    public int SetRangeRaw(string key, int offset, byte[] value)
    {
      return invoke("SETRANGE", 3,
        w => { w.WriteArg(key); w.WriteArg(offset); w.WriteArg(value); },
        r => r.ReadInt());
    }

    public int SetRangeFrom(string key, int offset, Stream source, int count)
    {
      return invoke("SETRANGE", 3,
        w => { w.WriteArg(key); w.WriteArg(offset); w.WriteArg(source, count); },
        r => r.ReadInt());
    }

    public int Strlen(string key)
    {
      return invoke("STRLEN", key, r => r.ReadInt());
    }

    #endregion

    #region Hashes

    public bool HDel(string key, string field)
    {
      return invoke("HDEL", 2,
        w => { w.WriteArg(key); w.WriteArg(field); },
        r => r.ReadBool());
    }

    public bool HExists(string key, string field)
    {
      return invoke("HEXISTS", 2,
        w => { w.WriteArg(key); w.WriteArg(field); },
        r => r.ReadBool());
    }

    public T HGet(string key, string field)
    {
      return invoke("HGET", 2,
        w => { w.WriteArg(key); w.WriteArg(field); },
        _readObj);
    }

    public byte[] HGetRaw(string key, string field)
    {
      return invoke("HGET", 2,
        w => { w.WriteArg(key); w.WriteArg(field); },
        r => r.ReadRawBulk());
    }

    public int HGetTo(string key, string field, Stream target)
    {
      return invoke("HGET", 2,
        w => { w.WriteArg(key); w.WriteArg(field); },
        r => r.ReadStreamedBulk(target));
    }

    public KeyValuePair<string, T>[] HGetAll(string key)
    {
      return invoke("HGETALL", key, r => r.ReadSerializedKeyValues(_serializer));
    }

    public long HIncrBy(string key, string field, long amount)
    {
      return invoke("HINCRBY", 3,
        w => { w.WriteArg(key); w.WriteArg(field); w.WriteArg(amount); },
        r => r.ReadInt64());
    }

    public string[] HKeys(string key)
    {
      return invoke("HKEYS", key, r => r.ReadStrMultiBulk());
    }

    public int HLen(string key)
    {
      return invoke("HLEN", key, r => r.ReadInt());
    }

    public T[] HMGet(string key, params string[] fields)
    {
      return invoke("HMGET", fields.Length + 1,
        w => { w.WriteArg(key); Array.ForEach(fields, w.WriteArg); },
        _readObjs);
    }

    public bool HMSet(string key, IEnumerable<KeyValuePair<string, T>> mappings)
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

    public bool HSet(string key, string field, T value)
    {
      return invoke("HSET", 3,
        w => { w.WriteArg(key); w.WriteArg(field); w.WriteArg(_serializer, value); },
        r => r.ReadBool());
    }

    public bool HSetRaw(string key, string field, byte[] data)
    {
      return invoke("HSET", 3,
        w => { w.WriteArg(key); w.WriteArg(field); w.WriteArg(data); },
        r => r.ReadBool());
    }

    public bool HSetFrom(string key, string field, Stream source, int count)
    {
      return invoke("HSET", 3,
        w => { w.WriteArg(key); w.WriteArg(field); w.WriteArg(source, count); },
        r => r.ReadBool());
    }

    public bool HSetNX(string key, string field, T value)
    {
      return invoke("HSETNX", 3,
        w => { w.WriteArg(key); w.WriteArg(field); w.WriteArg(_serializer, value); },
        r => r.ReadBool());
    }

    public T[] HVals(string key)
    {
      return invoke("HVALS", key, _readObjs);
    }

    #endregion

    #region Lists

    // a slight variation from Redis doc since params array must be last
    public KeyValuePair<string, T>? BLPop(int timeout, params string[] keys)
    {
      return invoke("BLPOP", keys.Length + 1,
        w => { w.WriteArg(timeout); Array.ForEach(keys, w.WriteArg); },
        r => r.ReadSerializedKeyValue(_serializer));
    }

    public KeyValuePair<string, T>? BRPop(int timeout, params string[] keys)
    {
      return invoke("BRPOP", keys.Length + 1,
        w => { w.WriteArg(timeout); Array.ForEach(keys, w.WriteArg); },
        r => r.ReadSerializedKeyValue(_serializer));
    }

    public T BRPopLPush(string src, string dest, int timeout)
    {
      return invoke("BRPOPLPUSH", 3,
        w => { w.WriteArg(src); w.WriteArg(dest); w.WriteArg(timeout); },
        _readObj);
    }

    public T LIndex(string key, int index)
    {
      return invoke("LINDEX", 2,
        w => { w.WriteArg(key); w.WriteArg(index); },
        _readObj);
    }

    public int LInsert(string key, T pivot, T value,
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

    public int LLen(string key)
    {
      return invoke("LLEN", key, r => r.ReadInt());
    }

    public T LPop(string key)
    {
      return invoke("LPOP", key, _readObj);
    }

    public int LPush(string key, T value)
    {
      return invoke("LPUSH", key, value, r => r.ReadInt());
    }

    public int LPushX(string key, T value)
    {
      return invoke("LPUSHX", key, value, r => r.ReadInt());
    }

    public T[] LRange(string key, int minIncl, int maxIncl)
    {
      return invoke("LRANGE", 3,
        w => { w.WriteArg(key); w.WriteArg(minIncl); w.WriteArg(maxIncl); },
        _readObjs);
    }

    public int LRem(string key, int count, T value)
    {
      return invoke("LREM", 3,
        w => { w.WriteArg(key); w.WriteArg(count); w.WriteArg(_serializer, value); },
        r => r.ReadInt());
    }

    public bool LSet(string key, int index, T value)
    {
      return invoke("LSET", 3,
        w => { w.WriteArg(key); w.WriteArg(index); w.WriteArg(_serializer, value); },
        r => r.ReadOk());
    }

    public bool LTrim(string key, int minIncl, int maxIncl)
    {
      return invoke("LTRIM", 3,
        w => { w.WriteArg(key); w.WriteArg(minIncl); w.WriteArg(maxIncl); },
        r => r.ReadOk());
    }

    public T RPop(string key)
    {
      return invoke("RPOP", key, _readObj);
    }

    public T RPopLPush(string srcKey, string destKey)
    {
      return invoke("RPOPLPUSH", 2,
        w => { w.WriteArg(srcKey); w.WriteArg(destKey); },
        _readObj);
    }

    public int RPush(string key, T value)
    {
      return invoke("RPUSH", key, value, r => r.ReadInt());
    }

    public int RPushX(string key, T value)
    {
      return invoke("RPUSHX", key, value, r => r.ReadInt());
    }

    #endregion

    #region Sets

    public bool SAdd(string key, T value)
    {
      return invoke("SADD", key, value, r => r.ReadBool());
    }

    public int SCard(string key)
    {
      return invoke("SCARD", key, r => r.ReadInt());
    }

    public T[] SDiff(params string[] keys)
    {
      return invoke("SDIFF", keys.Length,
        w => Array.ForEach(keys, w.WriteArg),
        _readObjs);
    }

    public bool SDiffStore(string destKey, params string[] keys)
    {
      return invoke("SDIFFSTORE", keys.Length + 1,
        w => { w.WriteArg(destKey); Array.ForEach(keys, w.WriteArg); },
        r => r.ReadOk());
    }

    public T[] SInter(params string[] keys)
    {
      return invoke("SINTER", keys.Length,
        w => Array.ForEach(keys, w.WriteArg),
        _readObjs);
    }

    public bool SInterStore(string destKey, params string[] keys)
    {
      return invoke("SINTERSTORE", keys.Length + 1,
        w => { w.WriteArg(destKey); Array.ForEach(keys, w.WriteArg); },
        r => r.ReadOk());
    }

    public bool SIsMember(string key, T value)
    {
      return invoke("SISMEMBER", key, value, r => r.ReadBool());
    }

    public T[] SMembers(string key)
    {
      return invoke("SMEMBERS", key, _readObjs);
    }

    public bool SMove(string srcKey, string destKey, T value)
    {
      return invoke("SMOVE", 3,
        w => { w.WriteArg(srcKey); w.WriteArg(destKey); w.WriteArg(_serializer, value); },
        r => r.ReadBool());
    }

    public T SPop(string key)
    {
      return invoke("SPOP", key, _readObj);
    }

    public T SRandMember(string key)
    {
      return invoke("SRANDMEMBER", key, _readObj);
    }

    public bool SRem(string key, T value)
    {
      return invoke("SREM", key, value, r => r.ReadBool());
    }

    public T[] SUnion(params string[] keys)
    {
      return invoke("SUNION", keys.Length,
        w => Array.ForEach(keys, w.WriteArg),
        _readObjs);
    }

    public bool SUnionStore(string destKey, params string[] keys)
    {
      return invoke("SUNIONSTORE", keys.Length + 1,
        w => { w.WriteArg(destKey); Array.ForEach(keys, w.WriteArg); },
        r => r.ReadOk());
    }

    #endregion

    #region Sorted Sets

    public bool ZAdd(string key, double score, T value)
    {
      return invoke("ZADD", 3,
        w => { w.WriteArg(key); w.WriteArg(score); w.WriteArg(_serializer, value); },
        r => r.ReadBool());
    }

    public int ZCard(string key)
    {
      return invoke("ZCARD", key, r => r.ReadInt());
    }

    public int ZCount(string key, double minIncl, double maxIncl)
    {
      return invoke("ZCOUNT", 3,
        w => { w.WriteArg(key); w.WriteArg(minIncl); w.WriteArg(maxIncl); },
        r => r.ReadInt());
    }

    public double ZIncrBy(string key, double amount, T value)
    {
      return invoke("ZINCRBY", 3,
        w => { w.WriteArg(key); w.WriteArg(amount); w.WriteArg(_serializer, value); },
        r => r.ReadDouble());
    }

    public int ZInterStore(string destKey, params string[] srcKeys)
    {
      return invoke("ZINTERSTORE", srcKeys.Length + 1,
        w => { w.WriteArg(destKey); Array.ForEach(srcKeys, w.WriteArg); },
        r => r.ReadInt());
    }

    public T[] ZRange(string key, int startRank, int endRank)
    {
      return invoke("ZRANGE", 3,
        w => { w.WriteArg(key); w.WriteArg(startRank); w.WriteArg(endRank); },
        _readObjs);
    }

    public KeyValuePair<T, double>[] ZRange(string key,
      int startRank, int endRank, bool withScores)
    {
      // revert to normal ZRange mode when withScores = false;
      if (!withScores)
        return kvBox(ZRange(key, startRank, endRank));

      return invoke("ZRANGE", 4,
        w =>
        {
          w.WriteArg(key); w.WriteArg(startRank); w.WriteArg(endRank);
          w.WriteArg("WITHSCORES");
        },
        r => r.ReadSerializedWithScores(_serializer));
    }

    public T[] ZRangeByScore(string key, double minIncl, double maxIncl,
      int? limitOffset = null, int? limitCount = null)
    {
      var includeLimit = (limitOffset.HasValue || limitCount.HasValue);

      return invoke("ZRANGEBYSCORE", (includeLimit ? 6 : 3),
        w =>
        {
          w.WriteArg(key); w.WriteArg(minIncl); w.WriteArg(maxIncl);
          if (!includeLimit) return;

          w.WriteArg("LIMIT");
          w.WriteArg(limitOffset.GetValueOrDefault(0));
          w.WriteArg(limitCount.GetValueOrDefault(int.MaxValue));
        },
        _readObjs);
    }

    public KeyValuePair<T, double>[] ZRangeByScore(string key,
      double minIncl, double maxIncl,
      bool withScores,
      int? limitOffset = null, int? limitCount = null)
    {
      // revert to normal ZRangeByScore when withScores = false
      if (!withScores)
        return kvBox(ZRangeByScore(key, minIncl, maxIncl, limitOffset, limitCount));

      var includeLimit = (limitOffset.HasValue || limitCount.HasValue);

      return invoke("ZRANGEBYSCORE", (includeLimit ? 7 : 4),
        w =>
        {
          w.WriteArg(key); w.WriteArg(minIncl); w.WriteArg(maxIncl);
          w.WriteArg("WITHSCORES");
          if (!includeLimit) return;

          w.WriteArg("LIMIT");
          w.WriteArg(limitOffset.GetValueOrDefault(0));
          w.WriteArg(limitCount.GetValueOrDefault(int.MaxValue));
        },
        r => r.ReadSerializedWithScores(_serializer));
    }

    public int ZRank(string key, T value)
    {
      return invoke("ZRANK", key, value, r => r.ReadInt());
    }

    public bool ZRem(string key, T value)
    {
      return invoke("ZREM", key, value, r => r.ReadBool());
    }

    public int ZRemRangeByRank(string key, int startRank, int endRank)
    {
      return invoke("ZREMRANGEBYRANK", 3,
        w => { w.WriteArg(key); w.WriteArg(startRank); w.WriteArg(endRank); },
        r => r.ReadInt());
    }

    public int ZRemRangeByScore(string key, double minIncl, double maxIncl)
    {
      return invoke("ZREMRANGEBYSCORE", 3,
        w => { w.WriteArg(key); w.WriteArg(minIncl); w.WriteArg(maxIncl); },
        r => r.ReadInt());
    }

    public T[] ZRevRange(string key, int startRank, int endRank)
    {
      return invoke("ZREVRANGE", 3,
        w => { w.WriteArg(key); w.WriteArg(startRank); w.WriteArg(endRank); },
        _readObjs);
    }

    public T[] ZRevRangeByScore(string key, double minIncl, double maxIncl)
    {
      return invoke("ZREVRANGEBYSCORE", 3,
        w => { w.WriteArg(key); w.WriteArg(minIncl); w.WriteArg(maxIncl); },
        _readObjs);
    }

    public int ZRevRank(string key, T value)
    {
      return invoke("ZREVRANK", key, value, r => r.ReadInt());
    }

    public double ZScore(string key, T value)
    {
      return invoke("ZSCORE", key, value, r => r.ReadDouble());
    }

    // complex arguments support for ZUnionStore
    public int ZUnionStore(string destKey, params string[] srcKeys)
    {
      return invoke("ZUNIONSTORE", srcKeys.Length + 1,
        w => { w.WriteArg(destKey); Array.ForEach(srcKeys, w.WriteArg); },
        r => r.ReadInt());
    }


    private KeyValuePair<T, double>[] kvBox(T[] values)
    {
      if (values == null) return null;

      var box = new KeyValuePair<T, double>[values.Length];
      for (var i = 0; i < values.Length; i++)
        box[i] = new KeyValuePair<T, double>(values[i], default(double));

      return box;
    }

    #endregion

    #region Pub/Sub

    public IObservable<Message<T>> PSubscribe(params string[] keys)
    {
      return pubsubAction("PSUBSCRIBE", keys, e => e.ActivePatterns.Add);
    }

    public int Publish(string channel, T msg)
    {
      return invoke("PUBLISH", channel, msg, r => r.ReadInt());
    }

    public IObservable<Message<T>> PUnsubscribe(params string[] keys)
    {
      return pubsubAction("PUNSUBSCRIBE", keys, e => e.ActivePatterns.Remove);
    }

    public IObservable<Message<T>> Subscribe(params string[] keys)
    {
      return pubsubAction("SUBSCRIBE", keys, e => e.ActiveChannels.Add);
    }

    public IObservable<Message<T>> Unsubscribe(params string[] keys)
    {
      return pubsubAction("UNSUBSCRIBE", keys, e => e.ActivePatterns.Remove);
    }


    private IObservable<Message<T>> pubsubAction(string command,
      string[] keys, Func<PubSubExecutor<T>, Func<string, bool>> executorAct)
    {
      var pe = Executor as PubSubExecutor<T>;
      if (pe == null)
        pe = SwitchExecutor(new PubSubExecutor<T>(_serializer,
          () => SwitchExecutor<ImmediateExecutor>()));

      Array.ForEach(keys, k => executorAct(pe)(k));


      invoke(command, keys.Length,
        w => Array.ForEach(keys, w.WriteArg),
        r => (object)null);

      return pe.GetOrBuildObservable();
    }

    #endregion


  }
}
