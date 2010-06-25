
using System;
using System.IO;
using System.Net.Sockets;

namespace Sider
{
  // see redis protocol specification for more info
  // http://code.google.com/p/redis/wiki/ProtocolSpecification
  public partial class RedisClient : IDisposable
  {
    private Socket _socket;
    private NetworkStream _stream;

    private RedisReader _reader;
    private RedisWriter _writer;


    public RedisClient(string host = "localhost", int port = 6379, int bufferSize = 4096)
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
