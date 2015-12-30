using System;
using System.Threading.Tasks;

namespace Sider {
  public interface IInvocation {
    void Write(RedisWriter writer);
    object Read(RedisReader reader);

    void SetResult(object result);
    void SetException(Exception exception);
  }

  public class Invocation<T> : IInvocation {
    readonly TaskCompletionSource<T> source;

    public Action<RedisWriter> WriteAction { get; private set; }
    public Func<RedisReader, T> ReadAction { get; private set; }

    public T Result {
      get { return source.Task.Result; }
    }

    public AggregateException Exception {
      get { return source.Task.Exception; }
    }

    public Invocation(
      Action<RedisWriter> writeAction,
      Func<RedisReader, T> readAction
    ) {
      source = new TaskCompletionSource<T>();

      WriteAction = writeAction;
      ReadAction = readAction;
    }

    public void Write(RedisWriter writer) {
      WriteAction(writer);
    }

    public object Read(RedisReader reader) {
      return ReadAction(reader);
    }

    public void SetResult(object result) {
      source.SetResult((T)result);
    }

    public void SetException(Exception exception) {
      source.SetException(exception);
    }
  }
}

