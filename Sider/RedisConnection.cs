using System;
using System.IO;
using System.Threading;

namespace Sider {
  // RedisConnection controls a Single connection to a Redis server.
  // The connection may only be used by a single thread at a time to avoid
  // unnecessary locks when synchronizing between read and write threads.
  public class RedisConnection : IDisposable {
    readonly Stream stream;
    readonly InvocationSink sink;
    readonly InvocationPump pump;

    internal Action<IInvocation> TestSubmitHook { get; set; }

    public RedisSettings Settings { get; private set; }
    
    public RedisConnection(Stream stream, RedisSettings settings) {
      if (stream == null) throw new ArgumentNullException("stream");
      if (settings == null) throw new ArgumentNullException("settings");

      this.stream = stream;
      sink = new InvocationSink(stream);
      pump = new InvocationPump(stream);

      Settings = settings;
    }

    public void Dispose() {
      sink.Dispose();
      pump.Dispose();
      stream.Dispose();
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

