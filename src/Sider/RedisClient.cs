
using System;
using System.IO;
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
  public partial class RedisClient<T> : RedisClientBase, IRedisClient<T>
  {
    // serialization stuff
    private readonly Func<ProtocolReader, T> _readObj;
    private readonly Func<ProtocolReader, T[]> _readObjs;

    private ISerializer<T> _serializer;


    public RedisClient(string host = RedisSettings.DefaultHost,
      int port = RedisSettings.DefaultPort) :
      this(RedisSettings.New().Host(host).Port(port)) { }

    public RedisClient(RedisSettings settings) :
      base(settings)
    {
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
    }

    // for testing only
    internal RedisClient(Stream incoming, Stream outgoing) :
      base(incoming, outgoing) { }
  }
}
