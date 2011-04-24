
namespace Sider.Executors
{
  public class ImmediateExecutor : IExecutor
  {
    public TResult Execute<TCmd, TResult>(TCmd command) where TCmd : Commands.Command<TResult>
    {
      throw new System.NotImplementedException();
    }
  }
}
