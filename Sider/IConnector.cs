namespace Sider {
  public interface IConnector {
    RedisSettings Settings { get; }

    IScope GetDefaultScope();
    IScope GetExclusiveScope();
  }
}

