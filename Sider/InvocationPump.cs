using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace Sider {
  public class InvocationPump : IDisposable {
    readonly RedisReader reader;
    readonly Queue<IInvocation> reads;

    readonly ManualResetEvent stopSignal;
    readonly ManualResetEvent stoppedSignal;

    public InvocationPump(Stream stream) {
      if (stream == null) throw new ArgumentNullException("stream");

      reader = new RedisReader(stream);
      reads = new Queue<IInvocation>();

      stopSignal = new ManualResetEvent(false);
      stoppedSignal = new ManualResetEvent(false);
      ThreadPool.QueueUserWorkItem(state => ProcessQueue());
    }

    public void Dispose() {
      stopSignal.Set();
      lock (reads) Monitor.Pulse(reads);
      stoppedSignal.WaitOne();

      stopSignal.Dispose();
      stoppedSignal.Dispose();
    }

    public void Queue(IInvocation invocation) {
      lock (reads) {
        reads.Enqueue(invocation);
        Monitor.Pulse(reads);
      }
    }

    void ProcessQueue() {
      try {
        while (!stopSignal.WaitOne(0)) {
          IInvocation invocation;
          lock (reads) {
            if (reads.Count == 0) {
              Monitor.Wait(reads);
              if (stopSignal.WaitOne(0)) return;
            }

            invocation = reads.Dequeue();
          }

          try {
            invocation.SetResult(invocation.Read(reader));
          } catch (Exception e) {
            invocation.SetException(e);
          }
        }

      } finally {
        stoppedSignal.Set();
      }
    }
  }
}

