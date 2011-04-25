
namespace Sider.Executors
{
  public class PubSubExecutor : ExecutorBase
  {
    public PubSubExecutor(IExecutor another) :
      base(another) { }


    public override T Execute<T>(Invocation<T> invocation)
    {
      throw new System.NotImplementedException();
    }
  }
}
