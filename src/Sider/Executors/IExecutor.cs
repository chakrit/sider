
namespace Sider.Executors
{
  public interface IExecutor
  {
    ProtocolReader Reader { get; }
    ProtocolWriter Writer { get; } 

    T Execute<T>(Invocation<T> invocation);
  }
}
