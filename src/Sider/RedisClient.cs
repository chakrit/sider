
using System.IO;
using System.Net.Sockets;
using System;
using System.Diagnostics;

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

    private bool _disposed;


    public bool IsDisposed { get { return _disposed; } }

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

    public RedisClient(Stream incoming, Stream outgoing)
    {
      _socket = null;
      _stream = null;

      _reader = new RedisReader(incoming);
      _writer = new RedisWriter(outgoing);
    }


    [Conditional("DEBUG")]
    private void ensureState()
    {
      Assert.IsTrue(!_disposed,
        () => new ObjectDisposedException(
          "RedisClient is disposed or is in an invalid state and is no longer usable."));
    }


    public void Dispose()
    {
      if (_disposed) return;

      _reader = null;
      _writer = null;

      if (_stream != null) {
        _stream.Close();
        _stream.Dispose();
        _stream = null;
      }

      if (_socket != null) {
        _socket.Close();
        _socket.Dispose();
        _socket = null;
      }

      _disposed = true;
    }
  }
}
