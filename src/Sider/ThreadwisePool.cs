
using System.Threading;

namespace Sider
{
  public class ThreadwisePool : IClientsPool
  {
    // TODO: LRU pruning since in peak time lots of clients will be built
    //   and dangling eating up memory, maybe we should change the _clientRef
    //   to ThreadLocal<WeakReference>

    // separate value for each thread... 
    private ThreadLocal<IRedisClient> _clientRef;
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
      _clientRef = new ThreadLocal<IRedisClient>(buildClient);

      _db = db;
    }


    public IRedisClient GetClient()
    {
      // gets thread-local value
      var client = _clientRef.Value;

      // rebuild the client if it's disposed
      if (client.IsDisposed)
        client = _clientRef.Value = buildClient();

      return client;
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
