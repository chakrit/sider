
namespace Sider.Executors
{
  public class PubSubExecutor : ExecutorBase
  {
    public PubSubExecutor(RedisSettings settings,
      ProtocolReader reader, ProtocolWriter writer) :
      base(settings, reader, writer) { }


    public override T Execute<T>(Invocation<T> invocation)
    {
      throw new System.NotImplementedException();
    }
  }
}
