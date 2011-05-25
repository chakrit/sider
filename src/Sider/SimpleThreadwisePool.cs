
using System;
using System.Threading;

namespace Sider
{
  public class SimpleThreadwisePool : SimpleThreadwisePool<string>
  {
    public SimpleThreadwisePool() :
      base(RedisSettings.Default) { }

    public SimpleThreadwisePool(
      Func<RedisSettings.Builder, RedisSettings> settingsFunc) :
      base(settingsFunc) { }

    public SimpleThreadwisePool(RedisSettings settings) :
      base(settings) { }
  }

  public class SimpleThreadwisePool<T> : SettingsWrapper, IClientsPool<T>
  {
    private ThreadLocal<StpContext> _contextStore;


    public SimpleThreadwisePool() :
      this(RedisSettings.Default) { }

    public SimpleThreadwisePool(
      Func<RedisSettings.Builder, RedisSettings> settingsFunc) :
      this(settingsFunc(RedisSettings.Build())) { }

    public SimpleThreadwisePool(RedisSettings settings) :
      base(settings)
    {
      _contextStore = new ThreadLocal<StpContext>();
    }


    public IRedisClient<T> GetClient()
    {
      var context = _contextStore.Value;
      if (context == null) {
        context = new StpContext();
        context.Client = buildClient();
        context.ClientThread = getCurrentThread();

        // IMPORTANT: need to be done *after* ClientThread assigned
        context.JoinThread = startWaitingThread(context);

        _contextStore.Value = context;
      }

      return context.Client;
    }


    // thread that waits until the main thread terminates so cleanup the context
    private void waitingThread(StpContext refContext)
    {
      refContext.ClientThread.Join();

      // ClientThread exited by this point
      refContext.Disposed = true;

      refContext.Client.Dispose();
      refContext.Client = null;
      refContext.ClientThread = null;
      refContext.JoinThread = null;
    }


    private IRedisClient<T> buildClient()
    {
      return new RedisClient<T>(Settings);
    }

    private Thread startWaitingThread(StpContext context)
    {
      var t = new Thread(() => waitingThread(context));
      t.Start();

      return t;
    }

    private Thread getCurrentThread()
    {
      return Thread.CurrentThread;
    }


    private class StpContext
    {
      public bool Disposed { get; set; }

      public IRedisClient<T> Client { get; set; }
      public Thread ClientThread { get; set; }
      public Thread JoinThread { get; set; }
    }
  }
}
