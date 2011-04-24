
using Sider.Executors;

namespace Sider
{
  public static class RedisCommands
  {
    public static Invocation<T> Get<T>(string key)
    {
      return Invocation.New(
        w =>
        {
          w.WriteTypeChar(ResponseType.MultiBulk);
          w.WriteLine("GET");
        },
        r => { });
    }
  }
}
