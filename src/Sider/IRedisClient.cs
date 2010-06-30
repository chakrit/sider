
using System;
using System.IO;

namespace Sider
{
  // see the redis commands reference for more info:
  // http://code.google.com/p/redis/wiki/CommandReference
  public interface IRedisClient : IDisposable
  {
    bool Ping();


    int Del(params string[] keys);
    string[] Keys(string pattern);
    bool Exists(string key);

    bool Set(string key, string value);
    bool SetRaw(string key, byte[] raw);
    bool SetFrom(string key, Stream source, int count);

    bool SetNX(string key, string value);

    string Get(string key);
    byte[] GetRaw(string key);
    int GetTo(string key, Stream target);

    string[] MGet(params string[] keys);

    long Incr(string key);


    int LPush(string key, string value);
    int RPush(string key, string value);

    string[] LRange(string key, int minInclusive, int maxInclusive);

    string LPop(string key);
    string RPop(string key);


    bool SAdd(string key, string value);
    bool SRem(string key, string value);

    string[] SMembers(string key);


    bool ZAdd(string key, double score, string value);
    bool ZRem(string key, string value);

    string[] ZRangeByScore(string key, double minInclusive, double maxInclusive);

    int ZRemRangeByScore(string key, double minInclusive, double maxInclusive);

    double ZIncrBy(string key, double amount, string value);
  }
}
