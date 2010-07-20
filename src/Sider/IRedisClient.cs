
using System;
using System.IO;
using System.Collections.Generic;

namespace Sider
{
  // see the redis commands reference for more info:
  // http://code.google.com/p/redis/wiki/CommandReference
  public interface IRedisClient : IDisposable
  {
    bool IsDisposed { get; }


    // connection handling / misc management
    bool Ping();
    bool Auth(string password);
    //IDictionary<string, string> Info();
    // INFO requires a special no-response-type-char read

    bool Save();
    bool BgSave();
    bool BgRewriteAOF();
    DateTime LastSave();

    void Quit();
    void Shutdown();


    // commands operating on all kinds of values
    bool Exists(string key);
    int Del(params string[] keys);
    RedisType Type(string key);
    string[] Keys(string pattern);
    string RandomKey();
    bool Rename(string oldKey, string newKey);
    bool RenameNX(string oldKey, string newKey);

    int DbSize();
    bool Expire(string key, TimeSpan span);
    bool ExpireAt(string key, DateTime time);
    TimeSpan TTL(string key);

    bool Select(int dbIndex);
    bool Move(string key, int dbIndex);
    bool FlushDB();
    bool FlushAll();


    // commands operating on string values
    bool Set(string key, string value);
    bool SetRaw(string key, byte[] raw);
    bool SetFrom(string key, Stream source, int count);

    string Get(string key);
    byte[] GetRaw(string key);
    int GetTo(string key, Stream target);

    string GetSet(string key, string value);
    string[] MGet(params string[] keys);

    bool SetNX(string key, string value);
    bool SetEX(string key, TimeSpan ttl, string value);

    // TODO: Require MultiBulk write support
    //bool MSet(IEnumerable<KeyValuePair<string, string>> mapping);
    //bool MSetNX(IEnumerable<KeyValuePair<string, string>> mapping);

    long Incr(string key);
    long IncrBy(string key, long value);
    long Decr(string key);
    long DecrBy(string key, long value);

    int Append(string key, string value);
    string Substr(string key, int start, int end);


    // commands operating on lists
    int RPush(string key, string value);
    int LPush(string key, string value);

    int LLen(string key);
    string[] LRange(string key, int minIncl, int maxIncl);
    bool LTrim(string key, int minIncl, int maxIncl);

    string LIndex(string key, int index);
    bool LSet(string key, int index, string value);
    int LRem(string key, int count, string value);

    string LPop(string key);
    string RPop(string key);

    // TODO: Require blocking semantic (needs to tinker with socket ReadTimeout)
    //KeyValuePair<string, string> BLPop(TimeSpan timeout, params string[] keys);
    //KeyValuePair<string, string> BRPop(TimeSpan timeout, params string[] keys);

    string RPopLPush(string srcKey, string destKey);

    // SORT <-- complicated 


    // commands operating on sets
    bool SAdd(string key, string value);
    bool SRem(string key, string value);
    string SPop(string key);
    bool SMove(string srcKey, string destKey, string value);

    int SCard(string key);
    bool SIsMember(string key, string value);

    string[] SInter(params string[] keys);
    bool SInterStore(string destKey, params string[] srcKeys);
    string[] SUnion(params string[] keys);
    bool SUnionStore(string destKey, params string[] srcKeys);
    string[] SDiff(params string[] keys);
    bool SDiffStore(string destKey, params string[] srcKeys);

    string[] SMembers(string key);
    string SRandMember(string key);


    // commands operating on sorted sets
    bool ZAdd(string key, double score, string value);
    bool ZRem(string key, string value);
    double ZIncrBy(string key, double amount, string value);

    int ZRank(string key, string value);
    int ZRevRank(string key, string value);

    // TODO: Ipmlement LIMIT and WITHSCORES
    string[] ZRange(string key, int startRank, int endRank);
    string[] ZRevRange(string key, int startRank, int endRank);
    string[] ZRangeByScore(string key, double minIncl, double maxIncl);

    int ZRemRangeByRank(string key, int startRank, int endRank);
    int ZRemRangeByScore(string key, double minIncl, double maxIncl);

    int ZCard(string key);
    double ZScore(string key, string value);

    // TODO: Implement WEIGHTS AGGREGATE SUM MIN MAX support
    int ZUnionStore(string destKey, params string[] srcKeys);
    int ZInterStore(string destKey, params string[] srcKeys);


    // commands operating on hashes
    bool HSet(string key, string field, string value);
    bool HSetRaw(string key, string field, byte[] data);
    bool HSetFrom(string key, string field, Stream source, int count);

    string HGet(string key, string field);
    byte[] HGetRaw(string key, string field);
    int HGetTo(string key, string field, Stream target);

    bool HSetNX(string key, string field, string value);
    string[] HMGet(string key, params string[] fields);
    long HIncrBy(string key, string field, long amount);

    bool HExists(string key, string field);
    bool HDel(string key, string field);
    int HLen(string key);

    string[] HKeys(string key);
    string[] HVals(string key);

    //IEnumerable<KeyValuePair<string, string>> HGetAll(string key);

    // TODO: HMSet Require MultiBulk write support

  }
}
