
using System;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using Sider.Executors;

namespace Sider
{
  public abstract class RedisClientBase : SettingsWrapper, IDisposable
  {
    private Socket _socket;
    private Stream _stream;
    private ProtocolReader _reader;
    private ProtocolEncoder _encoder;
    private ProtocolWriter _writer;

    private IExecutor _executor;

    private bool _disposing;
    private bool _disposed;


    public bool IsDisposed { get { return _disposed; } }
    protected bool IsDisposing { get { return _disposing; } }

    internal IExecutor Executor { get { return _executor; } }

    // expose reader and writer to IExecutors
    internal ProtocolReader Reader { get { return _reader; } }
    internal ProtocolWriter Writer { get { return _writer; } }


    public RedisClientBase(string host = RedisSettings.DefaultHost,
      int port = RedisSettings.DefaultPort) :
      this(RedisSettings.Build().Host(host).Port(port)) { }

    public RedisClientBase(RedisSettings settings) :
      base(settings)
    {
      _executor = new ImmediateExecutor();
      _executor.Init(this);

      Reset();
    }

    internal RedisClientBase(Stream incoming, Stream outgoing) :
      base(RedisSettings.Default)
    {
      _executor = new ImmediateExecutor();
      _executor.Init(this);

      _socket = null;
      _stream = null;

      _encoder = new ProtocolEncoder(Settings);
      _reader = new ProtocolReader(Settings, _encoder, incoming);
      _writer = new ProtocolWriter(Settings, _encoder, outgoing);
    }


    public virtual void Reset()
    {
      Debug.WriteLine("RESET");

      _disposing = _disposed = false;
      _socket = new Socket(AddressFamily.InterNetwork,
        SocketType.Stream, ProtocolType.Tcp);

      _socket.ReceiveBufferSize = Settings.ReadBufferSize;
      _socket.SendBufferSize = Settings.WriteBufferSize;
      _socket.NoDelay = true;

      // split timeout settings so both can be set separately
      _socket.ReceiveTimeout = Settings.ConnectionTimeout;
      _socket.SendTimeout = Settings.ConnectionTimeout;

      _socket.Connect(Settings.Host, Settings.Port);
      _stream = new NetworkStream(_socket, FileAccess.ReadWrite, true);

      _encoder = new ProtocolEncoder(Settings);
      _reader = new ProtocolReader(Settings, _encoder, _stream);
      _writer = new ProtocolWriter(Settings, _encoder, _stream);
    }


    internal T SwitchExecutor<T>() where T : IExecutor, new()
    { return SwitchExecutor(new T()); }

    internal T SwitchExecutor<T>(T newExecutor)
      where T : IExecutor
    {
      newExecutor.Init(previous: _executor);
      _executor.Dispose(); // after INIT to make sure the new executor works

      _executor = newExecutor;
      return newExecutor;
    }


    public virtual void Dispose()
    {
      if (_disposing || _disposed) return;
      _disposing = true;

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
