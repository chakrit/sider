
using System;

namespace Sider
{
  public class PubSubObservable<T> : Observable<T>
  {
    private RedisReader _reader;
    private Action _onDone;

    public PubSubObservable(Action onDone)
    {
    }
  }
}
