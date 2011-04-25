
namespace Sider.Executors
{
  public abstract class ExecutorBase : SettingsWrapper, IExecutor
  {
    public ProtocolReader Reader { get; private set; }
    public ProtocolWriter Writer { get; private set; }


    protected ExecutorBase(IExecutor another) :
      this(another.Settings, another.Reader, another.Writer) { }

    protected ExecutorBase(RedisSettings settings,
      ProtocolReader reader, ProtocolWriter writer) :
      base(settings)
    {
      Reader = reader;
      Writer = writer;
    }


    // TODO: Filter invalid commands
    public abstract T Execute<T>(Invocation<T> invocation);
  }
}
