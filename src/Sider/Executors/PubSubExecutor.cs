
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


    public override TInv Execute<TInv>(Invocation<TInv> invocation)
    {
      switch (invocation.Command) {
      case "SUBSCRIBE":
      case "PSUBSCRIBE":
      case "UNSUBSCRIBE":
      case "PUNSUBSCRIBE":
      case "QUIT": {
        invocation.WriteAction(Writer);
        Writer.Flush();
        return default(TInv);
      }
      }

      // check if the observable is done since by the point user executes the next
      // non-pubsub command the observable should have been disposed
      // (i.e. user leaving PUBSUB mode)
      if (_observable.SubscribersCount > 0 || !_observable.IsDisposed) {
        _observable.Dispose();
        _observable = null;

        // execute the supplied command as we're leaving pub/sub
        invocation.WriteAction(Writer);
        Writer.Flush();
        var result = invocation.ReadAction(Reader);

        // run the stop delegate (supposedly switching out PubSubExecutor)
        if (_onDone != null)
          _onDone();

        return result;
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
      private ManualResetEvent _event;

      public PubSubObservable(PubSubExecutor<T> parent)
      {
        _parent = parent;
        _event = new ManualResetEvent(false);
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

          Complete();
        }
        catch (Exception e) { Error(e); }
        finally {
          _event.Set();
          Dispose();
        }
      }


      public override void Dispose()
      {
        _event.WaitOne();

        if (_parent._onDone != null)
          _parent._onDone();

        base.Dispose();
      }
    }
  }
}
