
using System;
using System.Collections.Generic;
using System.IO;

namespace Sider
{
  // see the redis commands reference for more info:
  // http://redis.io/commands
  public interface IRedisClient : IDisposable
  {
    bool IsDisposed { get; }


    // NOTE: Please see the RedisClient.API.cs file for a proper sorted listing
    //   that matches with commands on redis.io site.
    //   as this file is extracted via a refactoring tool

    int Append(string key, string value);
    bool Auth(string password);
    bool BgRewriteAOF();
    bool BgSave();
    System.Collections.Generic.KeyValuePair<string, string>[] ConfigGet(string param);
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
    string Get(string key);
    int GetBit(string key, int offset);
    string GetRange(string key, int start, int end);
    byte[] GetRangeRaw(string key, int start, int end);
    int GetRangeTo(string key, int start, int end, System.IO.Stream source);
    byte[] GetRaw(string key);
    string GetSet(string key, string value);
    int GetTo(string key, System.IO.Stream target);
    bool HDel(string key, string field);
    bool HExists(string key, string field);
    string HGet(string key, string field);
    System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string, string>> HGetAll(string key);
    byte[] HGetRaw(string key, string field);
    int HGetTo(string key, string field, System.IO.Stream target);
    long HIncrBy(string key, string field, long amount);
    string[] HKeys(string key);
    int HLen(string key);
    string[] HMGet(string key, params string[] fields);
    bool HMSet(string key, System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string, string>> mappings);
    bool HSet(string key, string field, string value);
    bool HSetFrom(string key, string field, System.IO.Stream source, int count);
    bool HSetNX(string key, string field, string value);
    bool HSetRaw(string key, string field, byte[] data);
    string[] HVals(string key);
    long Incr(string key);
    long IncrBy(string key, long value);
    string[] Keys(string pattern);
    DateTime LastSave();
    string LIndex(string key, int index);
    int LLen(string key);
    string LPop(string key);
    int LPush(string key, string value);
    int LPushX(string key, string value);
    string[] LRange(string key, int minIncl, int maxIncl);
    int LRem(string key, int count, string value);
    bool LSet(string key, int index, string value);
    bool LTrim(string key, int minIncl, int maxIncl);
    string[] MGet(params string[] keys);
    bool Move(string key, int dbIndex);
    bool MSet(System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string, string>> mappings);
    bool MSetNX(System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string, string>> mappings);
    bool Persist(string key);
    bool Ping();
    void Quit();
    string RandomKey();
    bool Rename(string oldKey, string newKey);
    bool RenameNX(string oldKey, string newKey);
    void Reset();
    string RPop(string key);
    string RPopLPush(string srcKey, string destKey);
    int RPush(string key, string value);
    int RPushX(string key, string value);
    bool SAdd(string key, string value);
    bool Save();
    int SCard(string key);
    string[] SDiff(params string[] keys);
    bool SDiffStore(string destKey, params string[] keys);
    bool Select(int dbIndex);
    bool Set(string key, string value);
    int SetBit(string key, int offset, int value);
    bool SetEX(string key, TimeSpan ttl, string value);
    bool SetFrom(string key, System.IO.Stream source, int count);
    bool SetNX(string key, string value);
    int SetRange(string key, int offset, string value);
    int SetRangeFrom(string key, int offset, System.IO.Stream source, int count);
    int SetRangeRaw(string key, int offset, byte[] value);
    bool SetRaw(string key, byte[] raw);
    void Shutdown();
    string[] SInter(params string[] keys);
    bool SInterStore(string destKey, params string[] keys);
    bool SIsMember(string key, string value);
    string[] SMembers(string key);
    bool SMove(string srcKey, string destKey, string value);
    string SPop(string key);
    string SRandMember(string key);
    bool SRem(string key, string value);
    int Strlen(string key);
    string Substr(string key, int start, int end);
    string[] SUnion(params string[] keys);
    bool SUnionStore(string destKey, params string[] keys);
    TimeSpan TTL(string key);
    RedisType Type(string key);
    bool ZAdd(string key, double score, string value);
    int ZCard(string key);
    int ZCount(string key, double minIncl, double maxIncl);
    double ZIncrBy(string key, double amount, string value);
    int ZInterStore(string destKey, params string[] srcKeys);
    string[] ZRange(string key, int startRank, int endRank);
    string[] ZRangeByScore(string key, double minIncl, double maxIncl);
    int ZRank(string key, string value);
    bool ZRem(string key, string value);
    int ZRemRangeByRank(string key, int startRank, int endRank);
    int ZRemRangeByScore(string key, double minIncl, double maxIncl);
    string[] ZRevRange(string key, int startRank, int endRank);
    string[] ZRevRangeByScore(string key, double minIncl, double maxIncl);
    int ZRevRank(string key, string value);
    double ZScore(string key, string value);
    int ZUnionStore(string destKey, params string[] srcKeys);
  }
}
