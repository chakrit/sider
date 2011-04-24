
using System;
using Sider.Serialization;

namespace Sider
{
  public partial class InvocationBuilder<T> : SettingsWrapper
  {
    private ISerializer<T> _serializer;

    private readonly Func<ProtocolReader, T> _readObj;
    private readonly Func<ProtocolReader, T[]> _readObjs;



    public InvocationBuilder(RedisSettings settings) :
      base(settings)
    {
      if (settings.SerializerOverride != null) {
        // TODO: Convert ISerializer to use capability testing instead
        _serializer = settings.SerializerOverride as ISerializer<T>;
        SAssert.ArgumentSatisfy(() => settings, _ => _serializer != null,
          "Specified serializer is not compatible with specified generic type.");
      }
      else
        _serializer = Serializers.For<T>();

      _serializer.Init(settings);

      // frequently used delegates
      _readObj = r => r.ReadSerializedBulk(_serializer);
      _readObjs = r => r.ReadSerializedMultiBulk(_serializer);
    }


    private Invocation<TInv> invoke<TInv>(string command,
      Func<ProtocolReader, TInv> readAction)
    {
      return invoke<TInv>(command, 0, w => { }, readAction);
    }

    private Invocation<TInv> invoke<TInv>(string command, string key,
      Func<ProtocolReader, TInv> readAction)
    {
      return invoke<TInv>(command, 1, w => w.WriteArg(key), readAction);
    }

    private Invocation<TInv> invoke<TInv>(string command, string key, T value,
      Func<ProtocolReader, TInv> readAction)
    {
      return invoke<TInv>(command, 1,
        w => { w.WriteArg(key); w.WriteArg(_serializer, value); },
        readAction);
    }

    private Invocation<TInv> invoke<TInv>(
      string command, int numArgs,
      Action<ProtocolWriter> writeArgsAction,
      Func<ProtocolReader, TInv> readAction)
    {
      return Invocation.New(command, w =>
      {
        w.WriteCmdStart(command, numArgs);
        writeArgsAction(w);
      }, r =>
      {
        return readAction(r);
      });
    }
  }
}
