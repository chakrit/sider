
using System;
using System.Collections.Generic;
using System.IO;

namespace Sider
{
  // see the redis commands reference for more info:
  // http://redis.io/commands

  // default data type == string
  public interface IRedisClient : IRedisClient<string>
  {
  }

  public interface IRedisClient<T> : IDisposable
  {
    bool IsDisposed { get; }

    IEnumerable<object> Pipeline(Action<IRedisClient> pipelinedCalls);


    // NOTE: Please see the RedisClient.API.cs file for a proper sorted listing
    //   that matches with commands on redis.io site.
    //   as this file is extracted via a refactoring tool

    int Append(string key, T value);
    bool Auth(string password);
    bool BgRewriteAOF();
    bool BgSave();
    KeyValuePair<string, T>? BLPop(int timeout, params string[] keys);
    KeyValuePair<string, T>? BRPop(int timeout, params string[] keys);
    T BRPopLPush(string src, string dest, int timeout);
    KeyValuePair<string, string>[] ConfigGet(string param);
    bool ConfigResetStat();
    bool ConfigSet(string param, string value);
    int DbSize();
    string DebugObject(string key);
    void DebugSegfault();
    long Decr(string key);
    long DecrBy(string key, long value);
    int Del(params string[] keys);
    string Echo(string msg);
    bool Exists(string key);
    bool Expire(string key, TimeSpan span);
    bool ExpireAt(string key, DateTime time);
    bool FlushAll();
    bool FlushDb();
    T Get(string key);
    int GetBit(string key, int offset);
    T GetRange(string key, int start, int end);
    byte[] GetRangeRaw(string key, int start, int end);
    int GetRangeTo(string key, int start, int end, Stream source);
    byte[] GetRaw(string key);
    T GetSet(string key, T value);
    int GetTo(string key, Stream target);
    bool HDel(string key, string field);
    bool HExists(string key, string field);
    T HGet(string key, string field);
    IEnumerable<KeyValuePair<string, T>> HGetAll(string key);
    byte[] HGetRaw(string key, string field);
    int HGetTo(string key, string field, Stream target);
    long HIncrBy(string key, string field, long amount);
    string[] HKeys(string key);
    int HLen(string key);
    T[] HMGet(string key, params string[] fields);
    bool HMSet(string key, IEnumerable<KeyValuePair<string, T>> mappings);
    bool HSet(string key, string field, T value);
    bool HSetFrom(string key, string field, Stream source, int count);
    bool HSetNX(string key, string field, T value);
    bool HSetRaw(string key, string field, byte[] data);
    T[] HVals(string key);
    long Incr(string key);
    long IncrBy(string key, long value);
    string[] Keys(string pattern);
    DateTime LastSave();
    T LIndex(string key, int index);
    int LLen(string key);
    T LPop(string key);
    int LPush(string key, T value);
    int LPushX(string key, T value);
    T[] LRange(string key, int minIncl, int maxIncl);
    int LRem(string key, int count, T value);
    bool LSet(string key, int index, T value);
    bool LTrim(string key, int minIncl, int maxIncl);
    T[] MGet(params string[] keys);
    bool Move(string key, int dbIndex);
    bool MSet(IEnumerable<KeyValuePair<string, T>> mappings);
    bool MSetNX(IEnumerable<KeyValuePair<string, T>> mappings);
    bool Persist(string key);
    bool Ping();
    void Quit();
    string RandomKey();
    bool Rename(string oldKey, string newKey);
    bool RenameNX(string oldKey, string newKey);
    void Reset();
    T RPop(string key);
    T RPopLPush(string srcKey, string destKey);
    int RPush(string key, T value);
    int RPushX(string key, T value);
    bool SAdd(string key, T value);
    bool Save();
    int SCard(string key);
    T[] SDiff(params string[] keys);
    bool SDiffStore(string destKey, params string[] keys);
    bool Select(int dbIndex);
    bool Set(string key, T value);
    int SetBit(string key, int offset, int value);
    bool SetEX(string key, TimeSpan ttl, T value);
    bool SetFrom(string key, Stream source, int count);
    bool SetNX(string key, T value);
    int SetRange(string key, int offset, T value);
    int SetRangeFrom(string key, int offset, Stream source, int count);
    int SetRangeRaw(string key, int offset, byte[] value);
    bool SetRaw(string key, byte[] raw);
    void Shutdown();
    string[] SInter(params string[] keys);
    bool SInterStore(string destKey, params string[] keys);
    bool SIsMember(string key, T value);
    string[] SMembers(string key);
    bool SMove(string srcKey, string destKey, T value);
    string SPop(string key);
    string SRandMember(string key);
    bool SRem(string key, T value);
    int Strlen(string key);
    [Obsolete("No longer available since v2.0")]
    T Substr(string key, int start, int end);
    T[] SUnion(params string[] keys);
    bool SUnionStore(string destKey, params string[] keys);
    TimeSpan TTL(string key);
    RedisType Type(string key);
    bool ZAdd(string key, double score, T value);
    int ZCard(string key);
    int ZCount(string key, double minIncl, double maxIncl);
    double ZIncrBy(string key, double amount, T value);
    int ZInterStore(string destKey, params string[] srcKeys);
    T[] ZRange(string key, int startRank, int endRank);
    T[] ZRangeByScore(string key, double minIncl, double maxIncl);
    int ZRank(string key, T value);
    bool ZRem(string key, T value);
    int ZRemRangeByRank(string key, int startRank, int endRank);
    int ZRemRangeByScore(string key, double minIncl, double maxIncl);
    T[] ZRevRange(string key, int startRank, int endRank);
    T[] ZRevRangeByScore(string key, double minIncl, double maxIncl);
    int ZRevRank(string key, T value);
    double ZScore(string key, T value);
    int ZUnionStore(string destKey, params string[] srcKeys);
  }
}
