
using Sider.Commands;

namespace Sider.Executors
{
  public interface IExecutor
  {
    TResult Execute<TCmd, TResult>(TCmd command)
      where TCmd : ICommand<TResult>;
  }
}
