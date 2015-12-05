using System;

namespace Sider {
  public interface IScope : IDisposable {
    IRedisConnection Connection { get; }
  }
}

