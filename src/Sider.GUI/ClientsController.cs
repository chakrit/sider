
namespace Sider.GUI
{
  public class ClientsController
  {
    private IRedisClient<string> _client;

    private bool _invalidated;
    private string _host;
    private int _port;


    public string Host
    {
      get { return _host; }
      set { _host = value; _invalidated = true; }
    }

    public int Port
    {
      get { return _port; }
      set { _port = value; _invalidated = true; }
    }


    public ClientsController()
    {
      _invalidated = true;
    }


    public IRedisClient<string> GetClient()
    {
      if (_client == null || _client.IsDisposed || _invalidated)
        _client = new RedisClient(_host, _port);

      _invalidated = false;
      return _client;
    }
  }
}
