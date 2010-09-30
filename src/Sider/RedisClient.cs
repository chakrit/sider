
using System;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;

namespace Sider
{
  public partial class RedisClient : IRedisClient
  {
    private Socket _socket;
    private Stream _stream;

    private RedisSettings _settings;
    private RedisReader _reader;
    private RedisWriter _writer;

    private bool _disposed;


    public bool IsDisposed { get { return _disposed; } }

    public RedisClient(
      string host = RedisSettings.DefaultHost,
      int port = RedisSettings.DefaultPort) :
      this(new RedisSettings(host: host, port: port)) { }

    public RedisClient(RedisSettings settings)
    {
      SAssert.ArgumentNotNull(() => settings);

      _settings = settings;
      Reset();
    }

    // for testing only
    internal RedisClient(Stream incoming, Stream outgoing)
    {
      _socket = null;
      _stream = null;

      _reader = new RedisReader(incoming);
      _writer = new RedisWriter(outgoing);
    }

    public void Reset()
    {
      _socket = new Socket(AddressFamily.InterNetwork,
        SocketType.Stream,
        ProtocolType.Tcp);

      _socket.Connect(_settings.Host, _settings.Port);
      _stream = new NetworkStream(_socket, FileAccess.ReadWrite);

      _reader = new RedisReader(_stream);
      _writer = new RedisWriter(_stream);
    }


    [Conditional("DEBUG")]
    private void ensureNotDisposed()
    {
      SAssert.IsTrue(!_disposed,
        () => new ObjectDisposedException(
          "RedisClient is disposed or is in an invalid state and is no longer usable."));
    }

    // check in case Redis dropped idle connections
    private void ensureSocketWritable()
    {
      if (_socket == null)
        return;

      // reconnect if connection's dropped
      if (!_socket.Poll(_settings.SocketPollTimeouot, SelectMode.SelectWrite))
        Reset();
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
