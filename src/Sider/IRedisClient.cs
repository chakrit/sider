
using System;

namespace Sider
{
  public interface IRedisClient : IDisposable
  {
    bool Ping();

    int Del(params string[] keys);

    bool Set(string key, byte[] value);
    byte[] Get(string key);

    long Incr(string key);
  }
}
