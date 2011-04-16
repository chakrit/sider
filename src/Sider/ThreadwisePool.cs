
using System;
using System.Threading;

namespace Sider
{
  public class ThreadwisePool<T> : IClientsPool<T>
  {
    // TODO: LRU pruning since in peak time lots of clients will be built
    //   and dangling eating up memory, maybe we should change the _clientRef
    //   to ThreadLocal<WeakReference>

    // separate value for each thread... 
    private ThreadLocal<WeakReference> _threadRef;
    private RedisSettings _settings;

    private int? _db;


    protected RedisSettings Settings { get { return _settings; } }

    public ThreadwisePool(string host = RedisSettings.DefaultHost,
      int port = RedisSettings.DefaultPort,
      int? db = null) :
      this(RedisSettings.New().Host(host).Port(port), db) { }

    public ThreadwisePool(RedisSettings settings, int? db = null)
    {
      SAssert.ArgumentNotNull(() => settings);

      _settings = settings;
      _threadRef = new ThreadLocal<WeakReference>(buildClientInReference);

      _db = db;
    }


    public IRedisClient<T> GetClient()
    {
      // gets thread-local value
      var weakRef = _threadRef.Value;
      var client = (IRedisClient<T>)weakRef.Target;

      // rebuild the client if it's disposed
      if (client == null || client.IsDisposed) {
        client = BuildClient();
        _threadRef.Value = new WeakReference(client);
      }

      return client;
    }


    private WeakReference buildClientInReference()
    {
      return new WeakReference(BuildClient());
    }

    protected virtual IRedisClient<T> BuildClient()
    {
      var client = new RedisClient<T>(_settings);

      // TODO: Is there a better way than using a nullable here?
      // TODO: Move db selection out of BuildClient
      if (_db.HasValue)
        client.Select(_db.Value);

      return client;
    }
  }
}
