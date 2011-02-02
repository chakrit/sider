
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

    private byte[] _stringBuffer;

    private DateTime _lastWriteTime;
    private bool _disposing;
    private bool _disposed;


    public bool IsDisposed { get { return _disposed; } }

    public RedisClient(
      string host = RedisSettings.DefaultHost,
      int port = RedisSettings.DefaultPort) :
      this(new RedisSettings(host: host, port: port)) { }

    public RedisClient(RedisSettings settings)
    {
      SAssert.ArgumentNotNull(() => settings);

      _disposing = _disposed = false;

      _stringBuffer = new byte[settings.StringBufferSize];
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
      Debug.WriteLine("RESET");

      _socket = new Socket(AddressFamily.InterNetwork,
        SocketType.Stream,
        ProtocolType.Tcp);

      _socket.ReceiveBufferSize = _settings.ReadBufferSize;
      _socket.SendBufferSize = _settings.WriteBufferSize;
      _socket.NoDelay = true;

      _socket.Connect(_settings.Host, _settings.Port);

      _stream = new NetworkStream(_socket, FileAccess.ReadWrite, true);
      _reader = new RedisReader(_stream, _settings);
      _writer = new RedisWriter(_stream, _settings);

      _lastWriteTime = DateTime.Now;
    }


    [Conditional("DEBUG")]
    private void ensureNotDisposed()
    {
      SAssert.IsTrue(!_disposed,
        () => new ObjectDisposedException(
          "RedisClient is disposed or is in an invalid state and is no longer usable."));
    }


    public void Dispose()
    {
      if (_disposed || _disposing) return;
      _disposing = true;

      // attempt to properly shutdown the connection by sending a QUIT first
      try { Quit(); }
      catch (Exception) {
        // intentionally absorbed
      }

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
