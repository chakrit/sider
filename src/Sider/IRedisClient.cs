
using System;
using System.IO;

namespace Sider
{
  public interface IRedisClient : IDisposable
  {
    bool Ping();


    int Del(params string[] keys);
    bool Exists(string key);

    bool Set(string key, string value);
    bool SetRaw(string key, byte[] raw);
    bool SetFrom(string key, Stream source, int count);

    bool SetNX(string key, string value);

    string Get(string key);
    byte[] GetRaw(string key);
    int GetTo(string key, Stream target);

    long Incr(string key);


    bool SAdd(string key, string value);
    bool SRem(string key, string value);

    string[] SMembers(string key);


    bool ZAdd(string key, float score, string value);
    bool ZRem(string key, string value);

    int ZRemRangeByScore(string key, float minInclusive, float maxInclusive);

    float ZIncrBy(string key, float amount, string value);
  }
}
