using System;
using System.IO;
using System.Threading;

namespace Sider {
  // RedisConnection controls a Single connection to a Redis server.
  // The connection may only be used by a single thread at a time to avoid
  // unnecessary locks when synchronizing between read and write threads.
  public class RedisConnection : StreamWrapper {
    readonly InvocationSink sink;
    readonly InvocationPump pump;

    internal Action<IInvocation> TestSubmitHook { get; set; }

    public RedisConnection(Stream stream, RedisSettings settings)
      : base(stream, settings)
    {
      sink = new InvocationSink(Stream, settings);
      pump = new InvocationPump(Stream, settings);
    }

    public override void Dispose() {
      sink.Dispose();
      pump.Dispose();
      Stream.Dispose();
    }

    public void Submit(IInvocation invocation) {
      if (!Monitor.TryEnter(this)) throw new ConcurrentAccessException();

      if (TestSubmitHook != null) {
        TestSubmitHook(invocation);
        return;
      }

      try {
        sink.Queue(invocation);
        pump.Queue(invocation);
      } finally {
        Monitor.Exit(this);
      }
    }
  }
}

