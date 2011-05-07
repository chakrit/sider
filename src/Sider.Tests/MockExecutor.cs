
using Sider.Executors;

namespace Sider.Tests
{
  public class MockExecutor : IExecutor
  {
    public string LastInvokedCommand { get; private set; }

    public RedisClientBase Client { get; private set; }


    public void Init(RedisClientBase client) { Client = client; }

    public void Init(IExecutor previous)
    {
      throw new System.NotSupportedException();
    }


    public T Execute<T>(Invocation<T> invocation)
    {
      LastInvokedCommand = invocation.Command;
      return default(T);
    }


    public void Dispose() { }
  }
}
