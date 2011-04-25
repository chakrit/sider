
using System;
using System.Threading;

namespace Sider.Executors
{
  public class PubSubExecutor<T> : ExecutorBase
  {
    private PubSubObservable _observable;
    private ISerializer<T> _serializer;

    private Action _onDone;

    public PubSubExecutor(IExecutor another, ISerializer<T> serializer, Action onDone) :
      base(another)
    {
      _serializer = serializer;
      _onDone = onDone;
    }


    public override T Execute<T>(Invocation<T> invocation)
    {
      switch (invocation.Command) {
      case "SUBSCRIBE":
      case "PSUBSCRIBE":
      case "UNSUBSCRIBE":
      case "PUNSUBSCRIBE":
      case "QUIT": {
        invocation.WriteAction(Writer);
        return default(T);
      }
      }

      throw new InvalidOperationException(
        "Only (P)SUBSCRIBE/(P)UNSUBSCRIBE/QUIT allowed for a subscribing client.");
    }


    public IObservable<Message<T>> GetOrBuildObservable()
    {
      return _observable ?? (_observable = new PubSubObservable(this));
    }


    private class PubSubObservable : Observable<Message<T>>
    {
      private PubSubExecutor<T> _parent;
      int _channels;

      public PubSubObservable(PubSubExecutor<T> parent)
      {
        _parent = parent;
        _channels = 0;

        ThreadPool.QueueUserWorkItem(pubsubCore);
      }

      private void pubsubCore(object _)
      {
        try {
          while (!IsDisposed) {
            var msg = _parent.Reader.ReadMessage(_parent._serializer);
            Next(msg);

            if (msg.ChannelsCount == 0)
              break;
          }
        }
        catch (Exception e) { Error(e); }

        Complete();
        Dispose();

        if (_parent._onDone != null)
          _parent._onDone();
      }
    }
  }
}
