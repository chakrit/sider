using System.IO;

namespace Sider {
  public class InvocationSink : StreamWrapper {
    readonly RedisWriter writer;

    public InvocationSink(Stream stream, RedisSettings settings)
      : base(stream, settings) {
      writer = new RedisWriter(stream, settings);
    }

    public void Queue(IInvocation invocation) {
      invocation.Write(writer);
    }

    public override void Dispose() {
    }
  }
}

