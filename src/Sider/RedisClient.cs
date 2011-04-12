
using System;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using Sider.Serialization;

namespace Sider
{
  public class RedisClient : RedisClient<string>
  {
    public RedisClient(
      string host = RedisSettings.DefaultHost,
      int port = RedisSettings.DefaultPort) :
      base(host, port) { }

    internal RedisClient(Stream incoming, Stream outgoing) :
      base(incoming, outgoing) { }

    public RedisClient(RedisSettings settings) : base(settings) { }
  }

  public partial class RedisClient<T> : IRedisClient<T>
  {
    // TODO: Provide a way to safely configure ISerizlier<T> 
    //  (one should be selected on init, should not be settable while piplining etc.)
    private RedisSettings _settings;
    private ISerializer<T> _serializer;

    private Socket _socket;
    private Stream _stream;
    private RedisReader _reader;
    private RedisWriter _writer;

    private byte[] _stringBuffer;

    private bool _disposing;
    private bool _disposed;


    public bool IsDisposed { get { return _disposed; } }

    public RedisClient(string host = RedisSettings.DefaultHost,
      int port = RedisSettings.DefaultPort) :
      this(RedisSettings.New().Host(host).Port(port)) { }

    public RedisClient(RedisSettings settings)
    {
      SAssert.ArgumentNotNull(() => settings);

      _disposing = _disposed = false;

      if (settings.SerializerOverride != null) {
        _serializer = settings.SerializerOverride as ISerializer<T>;
        if (_serializer == null)
          throw new ArgumentException("Specified serializer is not compatible.");
      }
      else
        _serializer = Serializers.For<T>();

      _serializer.Init(_settings);

      _settings = settings;
      _stringBuffer = new byte[_settings.StringBufferSize];
      Reset();
    }

    // for testing only
    internal RedisClient(Stream incoming, Stream outgoing)
    {
      _socket = null;
      _stream = null;

      _settings = RedisSettings.Default;
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
