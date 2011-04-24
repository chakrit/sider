
namespace Sider.Executors
{
  public abstract class ExecutorBase : SettingsWrapper, IExecutor
  {
    public ProtocolReader Reader { get; private set; }
    public ProtocolWriter Writer { get; private set; }


    protected ExecutorBase(RedisSettings settings,
      ProtocolReader reader, ProtocolWriter writer) :
      base(settings)
    {
      Reader = reader;
      Writer = writer;
    }


    public abstract T Execute<T>(Invocation<T> invocation);
  }
}
