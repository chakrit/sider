namespace Sider {
  public interface IConnectionPool {
    RedisSettings Settings { get; }

    IScope GetSharedScope();
    IScope GetExclusiveScope();
  }
}

