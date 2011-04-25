
namespace Sider.Executors
{
  public class ImmediateExecutor : ExecutorBase
  {
    public ImmediateExecutor(IExecutor another) :
      base(another) { }

    public ImmediateExecutor(RedisSettings settings,
      ProtocolReader reader, ProtocolWriter writer) :
      base(settings, reader, writer) { }


    public override T Execute<T>(Invocation<T> invocation)
    {
      invocation.WriteAction(Writer);
      Writer.Flush();

      return invocation.ReadAction(Reader);
    }
  }
}
