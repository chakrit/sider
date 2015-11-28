
using System;
using System.Collections.Generic;
using System.Threading;
using Sider.Serialization;

namespace Sider.Executors
{
  internal class PubSubExecutor<T> : ExecutorBase
  {
    private PubSubObservable _observable;
    private ISerializer<T> _serializer;

    private bool _done;
    private Action _onDone;


    public override Mode Mode { get { return Mode.PubSub; } }
    
    public ISet<string> ActiveChannels { get; private set; }
    public ISet<string> ActivePatterns { get; private set; }

    public bool StillActive
    {
      get { return (ActiveChannels.Count + ActivePatterns.Count) > 0; }
    }

    public PubSubExecutor(ISerializer<T> serializer, Action onDone)
    {
      _serializer = serializer;
      _done = false;
      _onDone = onDone;

      ActiveChannels = new HashSet<string>();
      ActivePatterns = new HashSet<string>();
    }


    public override void Init(IExecutor previous)
    {
      if (previous is PipelinedExecutor)
        throw new InvalidOperationException(
          "Cannot enter subscription mode inside .Pipeline()");

      if (previous is TransactedExecutor)
        throw new InvalidOperationException(
          "Cannot enter subscription mode inside MULTI/EXEC block.");

      base.Init(previous);
    }


    public override TInv Execute<TInv>(Invocation<TInv> invocation)
    {
      switch (invocation.Command) {
      case "SUBSCRIBE":
      case "PSUBSCRIBE":
      case "UNSUBSCRIBE":
      case "PUNSUBSCRIBE": {
        var result = ExecuteImmediate(invocation);

        // run the done delegate (supposedly switching out PubSubExecutor)
        // after unsubscribed from all tracked channels
        if (!StillActive && _onDone != null) {
          _done = true;
          _onDone();
        }

        return result;
      }
      case "QUIT": {
        var result = ExecuteImmediate(invocation);

        _observable.Dispose();
        return result;
      }
      }

      throw new InvalidOperationException(
        "Only (P)SUBSCRIBE/(P)UNSUBSCRIBE/QUIT allowed for a subscribing client.\r\n");
    }


    public IObservable<Message<T>> GetOrBuildObservable()
    {
      if (_done) return null;

      return _observable ?? (_observable = new PubSubObservable(this));
    }


    public override void Dispose()
    {
      if (_observable != null && !_observable.IsDisposed && StillActive)
        throw new InvalidOperationException(
          "Cannot exit (P)SUBSCRIBE mode until (P)UNSUBSCRIBE-d from all channels.");

      _observable.Dispose();
      _observable = null;
      base.Dispose();
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

            if (msg.ChannelsCount.HasValue && msg.ChannelsCount.Value == 0)
              break;
          }

          Complete();
        }
        catch (Exception e) { Error(e); }
        finally { _event.Set(); }
      }


      public override void Dispose()
      {
        _event.WaitOne();
        base.Dispose();
      }
    }
  }
}
