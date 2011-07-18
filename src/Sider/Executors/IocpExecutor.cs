
using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Sider.Executors
{
  internal class IocpExecutor : ExecutorBase
  {
    private IProducerConsumerCollection<IocpTask> _writes, _reads;

    private ManualResetEventSlim _stopSignal;
    private Thread _writeThread;
    private Thread _readThread;

    public IocpExecutor()
    {
      _writes = new ConcurrentQueue<IocpTask>();
      _reads = new ConcurrentQueue<IocpTask>();

      _stopSignal = new ManualResetEventSlim(false);
      _writeThread = new Thread(writeThread);
      _readThread = new Thread(readThread);

      _writeThread.Start();
      _readThread.Start();
    }

    public override T Execute<T>(Invocation<T> invocation)
    {
      var result = default(T);

      // queue up the task
      IocpTask task = null;
      task = new IocpTask(
        () => invocation.WriteAction(Writer),
        () =>
        {
          result = invocation.ReadAction(Reader);
          task.WaitHandle.Set();
        });

      _writes.TryAdd(task);

      // wait for the task to finish
      task.WaitHandle.Wait(-1);
      task.WaitHandle.Dispose();
      return result;
    }


    // TODO: Handle cases where TryAdd/TryTake returns false?
    private void writeThread()
    {
      while (!_stopSignal.Wait(0)) {
        IocpTask task;
        if (!_writes.TryTake(out task))
          continue;

        task.WriteAction.Invoke();

        // TODO: This is wrong, we shouldn't need to care about reads
        //   and this'll make the read in wrong order. reads and writes
        //   should have separate progress pointer (but share the same queue
        //   since the order of reads must be the same as that of writes)
        if (!_reads.TryAdd(task))
          continue;
      }
    }

    private void readThread()
    {
      while (!_stopSignal.Wait(0)) {
        IocpTask task;
        if (!_reads.TryTake(out task))
          continue;

        task.ReadAction.Invoke();
      }
    }


    public override void Dispose()
    {
      _stopSignal.Set();
      _writeThread.Join();
      _readThread.Join();
      _stopSignal.Dispose();

      _writeThread = null;
      _readThread = null;

      _writes = null;
      _reads = null;
    }


    public class IocpTask
    {
      public ManualResetEventSlim WaitHandle { get; private set; }
      public Action WriteAction { get; private set; }
      public Action ReadAction { get; private set; }

      public IocpTask(Action writeAction, Action readAction)
      {
        WaitHandle = new ManualResetEventSlim(false);

        WriteAction = writeAction;
        ReadAction = readAction;
      }
    }
  }
}
