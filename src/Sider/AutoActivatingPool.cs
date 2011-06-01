
using System;
using System.Threading;

namespace Sider
{
  public class AutoActivatingPool : AutoActivatingPool<string>
  {
    public AutoActivatingPool() :
      base(RedisSettings.Default) { }

    public AutoActivatingPool(
      Func<RedisSettings.Builder, RedisSettings> settingsFunc) :
      base(settingsFunc) { }

    public AutoActivatingPool(RedisSettings settings) :
      base(settings) { }
  }

  public class AutoActivatingPool<T> : SettingsWrapper, IClientsPool<T>
  {
    private ThreadLocal<PoolContext> _contextStore;


    public AutoActivatingPool() :
      this(RedisSettings.Default) { }

    public AutoActivatingPool(
      Func<RedisSettings.Builder, RedisSettings> settingsFunc) :
      this(settingsFunc(RedisSettings.Build())) { }

    public AutoActivatingPool(RedisSettings settings) :
      base(settings)
    {
      _contextStore = new ThreadLocal<PoolContext>();
    }


    public IRedisClient<T> GetClient()
    {
      var context = _contextStore.Value;
      if (context == null) {
        context = new PoolContext();
        context.Client = buildClient();
        context.ClientThread = getCurrentThread();

        // IMPORTANT: need to be done *after* ClientThread assigned
        context.JoinThread = startWaitingThread(context);

        _contextStore.Value = context;
      }

      return context.Client;
    }


    // thread that waits until the main thread terminates so cleanup the context
    private void waitingThread(PoolContext refContext)
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

    private Thread startWaitingThread(PoolContext context)
    {
      var t = new Thread(() => waitingThread(context));
      t.Start();

      return t;
    }

    private Thread getCurrentThread()
    {
      return Thread.CurrentThread;
    }


    private class PoolContext
    {
      public bool Disposed { get; set; }

      public IRedisClient<T> Client { get; set; }
      public Thread ClientThread { get; set; }
      public Thread JoinThread { get; set; }
    }
  }
}
