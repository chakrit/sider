
using System;

namespace Sider.Executors
{
  public sealed class Invocation
  {
    public static Invocation<T> New<T>(Action<RedisWriter> writer,
      Func<RedisReader, T> reader)
    {
      return new Invocation<T>(writer, reader);
    }
  }

  public class Invocation<TResult>
  {
    public Action<RedisWriter> WriteAction { get; private set; }
    public Func<RedisReader, TResult> ReadAction { get; private set; }

    public Invocation(Action<RedisWriter> writer, Func<RedisReader, TResult> reader)
    {
      WriteAction = writer;
      ReadAction = reader;
    }
  }
}
