using System;

namespace Sider {
  public interface IScope : IDisposable {
    RedisConnection Connection { get; }
  }
}

