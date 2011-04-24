
using System;

namespace Sider
{
  internal class PubSubObservable<T> : Observable<T>
  {
    private RedisReader _reader;
    private Action _onDone;

    public PubSubObservable(Action onDone) :
      base(onDone)
    {
    }
  }
}
