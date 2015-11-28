
using System;

namespace Sider.Executors
{
  internal interface IExecutor : IDisposable
  {
    RedisClientBase Client { get; }
    Mode Mode { get; }

    void Init(RedisClientBase client);
    void Init(IExecutor previous);

    T Execute<T>(Invocation<T> invocation);
  }
}
