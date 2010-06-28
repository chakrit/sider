
using System;

namespace Sider
{
  public class ThreadwisePool : IClientsPool
  {
    [ThreadStatic] // separate value for each thread
    private WeakReference _client;


    private string _host;
    private int _port;

    public ThreadwisePool(string host = RedisClient.DefaultHost,
      int port = RedisClient.DefaultPort)
    {
      _host = host;
      _port = port;
    }


    public IRedisClient GetClient()
    {
      var client = _client.Target ??
        (_client = new WeakReference(buildClient()));

      return (IRedisClient)client;
    }


    private IRedisClient buildClient()
    {
      return new RedisClient(_host, _port);
    }
  }
}
