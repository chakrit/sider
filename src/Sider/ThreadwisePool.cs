
using System.Threading;

namespace Sider
{
  public class ThreadwisePool : IClientsPool
  {
    // separate value for each thread... 
    // TODO: WeakReference? does this scales-down?
    private ThreadLocal<IRedisClient> _clientRef;

    private RedisSettings _settings;


    public ThreadwisePool(string host = RedisSettings.DefaultHost,
      int port = RedisSettings.DefaultPort) :
      this(new RedisSettings(host: host, port: port)) { }

    public ThreadwisePool(RedisSettings settings)
    {
      SAssert.ArgumentNotNull(() => settings);

      _settings = settings;

      _clientRef = new ThreadLocal<IRedisClient>(buildClient);
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
      return new RedisClient(_settings);
    }
  }
}
