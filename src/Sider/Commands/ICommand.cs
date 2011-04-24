
using System;

namespace Sider.Commands
{
  public interface ICommand<TResult>
  {
    string Name { get; }

    void GetActions(out Action<RedisWriter> writeAction, out Func<RedisReader, TResult> readAction);

    Action<RedisReader> ReadAction { get; }
    Action<RedisWriter> WriteAction { get; }
  }
}
