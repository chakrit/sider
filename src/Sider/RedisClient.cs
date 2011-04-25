
using System;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using Sider.Executors;
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

  // TODO: Provide a way to safely configure ISerializer<T> 
  //  (one should be selected on init, should not be settable while piplining etc.)
  public partial class RedisClient<T> : SettingsWrapper, IRedisClient<T>
  {
    // frequently used delegates
    private readonly Func<ProtocolReader, T> _readObj;
    private readonly Func<ProtocolReader, T[]> _readObjs;

    // infrastructure
    private Socket _socket;
    private Stream _stream;
    private ProtocolReader _reader;
    private ProtocolWriter _writer;
    private ProtocolEncoder _encoder;

    private ISerializer<T> _serializer;
    private IExecutor _executor;


    private byte[] _stringBuffer;

    private bool _disposing;
    private bool _disposed;


    public bool IsDisposed { get { return _disposed; } }

    public RedisClient(string host = RedisSettings.DefaultHost,
      int port = RedisSettings.DefaultPort) :
      this(RedisSettings.New().Host(host).Port(port)) { }

    public RedisClient(RedisSettings settings) :
      base(settings)
    {
      _disposing = _disposed = false;
      _stringBuffer = new byte[Settings.EncodingBufferSize];

      if (settings.SerializerOverride != null) {
        _serializer = settings.SerializerOverride as ISerializer<T>;
        if (_serializer == null)
          throw new ArgumentException("Specified serializer is not compatible.");
      }
      else
        _serializer = Serializers.For<T>();

      _serializer.Init(Settings);
      _readObj = r => r.ReadSerializedBulk(_serializer);
      _readObjs = r => r.ReadSerializedMultiBulk(_serializer);

      // connect
      Reset();
    }

    // for testing only
    internal RedisClient(Stream incoming, Stream outgoing) :
      base(RedisSettings.Default)
    {
      _socket = null;
      _stream = null;

      _encoder = new ProtocolEncoder(Settings);
      _reader = new ProtocolReader(Settings, _encoder, incoming);
      _writer = new ProtocolWriter(Settings, _encoder, outgoing);
    }

    public void Reset()
    {
      Debug.WriteLine("RESET");

      _socket = new Socket(AddressFamily.InterNetwork,
        SocketType.Stream,
        ProtocolType.Tcp);

      _socket.ReceiveBufferSize = Settings.ReadBufferSize;
      _socket.SendBufferSize = Settings.WriteBufferSize;
      _socket.NoDelay = true;

      _socket.Connect(Settings.Host, Settings.Port);

      _stream = new NetworkStream(_socket, FileAccess.ReadWrite, true);
      _encoder = new ProtocolEncoder(Settings);
      _reader = new ProtocolReader(Settings, _encoder, _stream);
      _writer = new ProtocolWriter(Settings, _encoder, _stream);

      _executor = new ImmediateExecutor(Settings, _reader, _writer);
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
