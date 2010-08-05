
using System.Threading;

namespace Sider
{
  public class ThreadwisePool : IClientsPool
  {
    // separate value for each thread... 
    // TODO: WeakReference? does this scales-down?
    private ThreadLocal<IRedisClient> _clientRef;


    private string _host;
    private int _port;

    public ThreadwisePool(string host = RedisClient.DefaultHost,
      int port = RedisClient.DefaultPort)
    {
      _host = host;
      _port = port;

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
      return new RedisClient(_host, _port);
    }
  }
}
