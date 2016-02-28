using System;
using System.Threading;

namespace Sider
{
  public class ThreadwisePool : ThreadwisePool<string>
  {
    public ThreadwisePool(
      Func<RedisSettings.Builder, RedisSettings> settingsFunc) :
      base(settingsFunc) { }

    public ThreadwisePool(string host = RedisSettings.DefaultHost,
      int port = RedisSettings.DefaultPort,
      int? db = null) :
      base(host, port, db) { }

    public ThreadwisePool(RedisSettings settings, int? db = null) :
      base(settings, db) { }
  }

  public class ThreadwisePool<T> : IClientsPool<T>
  {
    // TODO: proper LRU pruning instead of ThreadLocal<WeakReference>

    // separate value for each thread... 
    private ThreadLocal<WeakReference> _threadRef;
    private RedisSettings _settings;

    private int? _db;


    protected RedisSettings Settings { get { return _settings; } }


    public ThreadwisePool(
      Func<RedisSettings.Builder, RedisSettings> settingsFunc) :
      this(settingsFunc(RedisSettings.Build())) { }

    public ThreadwisePool(string host = RedisSettings.DefaultHost,
      int port = RedisSettings.DefaultPort,
      int? db = null) :
      this(RedisSettings.Build().Host(host).Port(port), db) { }

    public ThreadwisePool(RedisSettings settings, int? db = null)
    {
      SAssert.ArgumentNotNull(() => settings);

      _settings = settings;
      _threadRef = new ThreadLocal<WeakReference>(BuildWeakReferenceClient);

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


    protected virtual WeakReference BuildWeakReferenceClient()
    {
      return new WeakReference(BuildClient());
    }

    protected virtual IRedisClient<T> BuildClient()
    {
      var client = new RedisClient<T>(_settings);

      if (!string.IsNullOrEmpty(_settings.Password))
        client.Auth(_settings.Password);

      // TODO: Is there a better way than using a nullable here?
      if (_db.HasValue)
        client.Select(_db.Value);

      return client;
    }
  }
}
