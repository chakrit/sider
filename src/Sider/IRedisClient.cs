
using System;
using System.IO;

namespace Sider
{
  public interface IRedisClient : IDisposable
  {
    bool Ping();

    int Del(params string[] keys);

    bool Set(string key, string value);
    bool SetRaw(string key, byte[] raw);
    bool SetFrom(string key, Stream source, int count);

    string Get(string key);
    byte[] GetRaw(string key);
    int GetTo(string key, Stream target);

    long Incr(string key);
  }
}
