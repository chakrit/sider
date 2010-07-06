
using System.IO;
using System.Net.Sockets;

namespace Sider
{
  public partial class RedisClient : IRedisClient
  {
    public const string DefaultHost = "localhost";
    public const int DefaultPort = 6379;


    private Socket _socket;
    private Stream _stream;

    private RedisReader _reader;
    private RedisWriter _writer;


    public RedisClient(string host = DefaultHost, int port = DefaultPort)
    {
      _socket = new Socket(AddressFamily.InterNetwork,
        SocketType.Stream,
        ProtocolType.Tcp);

      _socket.Connect(host, port);

      _stream = new NetworkStream(_socket, FileAccess.ReadWrite);

      _reader = new RedisReader(_stream);
      _writer = new RedisWriter(_stream);
    }


    public void Dispose()
    {
      _reader = null;
      _writer = null;

      _stream.Close();
      _socket.Close();

      _stream.Dispose();
      _socket.Dispose();
    }
  }
}
