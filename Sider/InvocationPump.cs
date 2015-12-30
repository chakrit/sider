using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace Sider {
  public class InvocationPump : StreamWrapper {
    readonly RedisReader reader;
    readonly Queue<IInvocation> reads;

    readonly ManualResetEvent stopSignal;
    readonly ManualResetEvent stoppedSignal;

    public InvocationPump(Stream stream, RedisSettings settings)
      : base(stream, settings) {
      if (stream == null) throw new ArgumentNullException("stream");
      if (settings == null) throw new ArgumentNullException("settings");

      reader = new RedisReader(stream, settings);
      reads = new Queue<IInvocation>();

      stopSignal = new ManualResetEvent(false);
      stoppedSignal = new ManualResetEvent(false);
      ThreadPool.QueueUserWorkItem(state => ProcessQueue());
    }

    public override void Dispose() {
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

