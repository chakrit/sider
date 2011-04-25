
namespace Sider.Executors
{
  public interface IExecutor
  {
    RedisSettings Settings { get; }
    ProtocolReader Reader { get; }
    ProtocolWriter Writer { get; }

    T Execute<T>(Invocation<T> invocation);
  }
}
