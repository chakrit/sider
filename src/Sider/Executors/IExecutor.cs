
namespace Sider.Executors
{
  public interface IExecutor
  {
    T Execute<T>(Invocation<T> invocation);
  }
}
