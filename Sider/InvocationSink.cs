using System;
using System.IO;

namespace Sider {
  public class InvocationSink : IDisposable {
    readonly RedisWriter writer;

    public InvocationSink(Stream stream) {
      if (stream == null) throw new ArgumentNullException("stream");

      writer = new RedisWriter(stream);
    }

    public void Queue(IInvocation invocation) {
      invocation.Write(writer);
    }

    public void Dispose() {
      
    }
  }
}

