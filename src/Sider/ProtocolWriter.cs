
using System;
using System.IO;

namespace Sider
{
  // A writer that's a bit more aware of the redis protocol than RedisWriter
  public class ProtocolWriter : SettingsWrapper
  {
    private Stream _stream;
    private RedisWriter _writer;
    private ProtocolEncoder _encoder;

    public ProtocolWriter(RedisSettings settings, ProtocolEncoder encoder,
      Stream stream) :
      base(settings)
    {
      _stream = stream;
      _writer = new RedisWriter(stream, settings);
      _encoder = encoder;
    }


    public void WriteCmdStart(string command, int numArgs)
    {
      _writer.WriteTypeChar(ResponseType.MultiBulk);
      _writer.WriteLine(numArgs + 1);

      _writer.WriteTypeChar(ResponseType.Bulk);
      _writer.WriteLine(command.Length);
      _writer.WriteLine(command);
    }

    public void WriteArg(string data)
    {
      var arr = _encoder.Encode(data);

      writeBulkStart(arr.Count);
      _writer.WriteBulk(arr.Array, arr.Offset, arr.Count);
    }

    // TODO: Number serialization?
    public void WriteArg(int num) { WriteArg(num.ToString()); }
    public void WriteArg(long num) { WriteArg(num.ToString()); }
    public void WriteArg(double num) { WriteArg(_encoder.Encode(num)); }

    public void WriteArg<T>(ISerializer<T> serializer, T value)
    {
      var count = serializer.GetBytesNeeded(value);

      writeBulkStart(count);
      _writer.WriteSerializedBulk<T>(serializer, value, count);
    }

    public void WriteArg(Stream source, int count)
    {
      writeBulkStart(count);
      _writer.WriteBulkFrom(source, count);
    }

    public void WriteArg(byte[] raw)
    {
      writeBulkStart(raw.Length);
      _writer.WriteBulk(raw);
    }

    public void WriteArg(ArraySegment<byte> raw)
    {
      writeBulkStart(raw.Count);
      _writer.WriteBulk(raw.Array, raw.Offset, raw.Count);
    }

    // TODO: Use the shared buffer?
    public void WriteArg(TimeSpan span) { WriteArg(_encoder.Encode(span)); }
    public void WriteArg(DateTime dt) { WriteArg(_encoder.Encode(dt)); }


    private void writeBulkStart(int count)
    {
      _writer.WriteTypeChar(ResponseType.Bulk);
      _writer.WriteLine(count);
    }
  }
}
