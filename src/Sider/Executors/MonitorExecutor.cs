
namespace Sider.Executors
{
  public class MonitorExecutor : ExecutorBase
  {
    public MonitorExecutor(RedisSettings settings,
      ProtocolReader reader, ProtocolWriter writer) :
      base(settings, reader, writer) { }


    public override T Execute<T>(Invocation<T> invocation)
    {
      // should just execute a MONITOR and then throw InvalidOperation for
      // everything else
      throw new System.NotImplementedException();
    }
  }
}
