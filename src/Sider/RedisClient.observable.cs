

namespace Sider
{
  public partial class RedisClient<T>
  {
    private bool _isObserving;
    private Observable<T> _observable;
    private Observable<string> _strObservable;


    //private IObservable<T> beginObserving()
    //{
    //  _isObserving = true;
    //  return _observable = new Observable<T>();
    //}

    //private IObservable<string> beginObservingStr()
    //{
    //  _isObserving = true;
    //  return _strObservable = new Observable<string>();
    //}

    //private void endObserving()
    //{
    //  _isObserving = false;
    //  if (_observable != null) {
    //    _observable.Dispose();
    //    _observable = null;
    //  }
    //}
  }
}
