
using System;
using System.Threading;

namespace Sider
{
  public class ThreadwisePool : IClientsPool
  {
    // TODO: LRU pruning since in peak time lots of clients will be built
    //   and dangling eating up memory, maybe we should change the _clientRef
    //   to ThreadLocal<WeakReference>

    // separate value for each thread... 
    private ThreadLocal<WeakReference> _threadRef;
    private RedisSettings _settings;

    private int? _db;


    public ThreadwisePool(string host = RedisSettings.DefaultHost,
      int port = RedisSettings.DefaultPort,
      int? db = null) :
      this(new RedisSettings(host: host, port: port), db) { }

    public ThreadwisePool(RedisSettings settings, int? db = null)
    {
      SAssert.ArgumentNotNull(() => settings);

      _settings = settings;
      _threadRef = new ThreadLocal<WeakReference>(buildClientInReference);

      _db = db;
    }


    public IRedisClient GetClient()
    {
      // gets thread-local value
      var weakRef = _threadRef.Value;
      var client = (IRedisClient)weakRef.Target;

      // rebuild the client if it's disposed
      if (client == null || client.IsDisposed) {
        client = buildClient();
        _threadRef.Value = new WeakReference(client);
      }

      return client;
    }


    private WeakReference buildClientInReference()
    {
      return new WeakReference(buildClient());
    }

    private IRedisClient buildClient()
    {
      var client = new RedisClient(_settings);

      // TODO: Is there a better way than using a nullable here?
      if (_db.HasValue)
        client.Select(_db.Value);

      return client;
    }
  }
}
