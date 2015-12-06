using System;
using System.IO;

namespace Sider {
  // RedisConnection controls a Single connection to a Redis server.
  // The connection may only be used by a single thread at a time to avoid
  // unnecessary locks when synchronizing between read and write threads.
  public class RedisConnection : IDisposable {
    readonly Stream stream;
    readonly InvocationSink sink;
    readonly InvocationPump pump;

    public RedisSettings Settings { get; private set; }
    
    public RedisConnection(Stream stream, RedisSettings settings) {
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

    // TODO: Protect against concurrent access, without slowing down.
    public void Submit(IInvocation invocation) {
      sink.Queue(invocation);
      pump.Queue(invocation);
    }
  }
}

