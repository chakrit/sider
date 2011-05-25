
using System;
using System.Text;

namespace Sider
{
  public class ProtocolEncoder : SettingsWrapper
  {
    protected const long UnixEpochL = 621355968000000000L; // 1st Jan 1970
    protected static readonly DateTime UnixEpoch = new DateTime(UnixEpochL);


    private byte[] _buffer;
    private Encoding _encoding;

    public byte[] SharedBuffer { get { return _buffer; } }

    public ProtocolEncoder(RedisSettings settings) :
      base(settings)
    {
      _buffer = new byte[settings.EncodingBufferSize];
      _encoding = settings.EncodingOverride ?? Encoding.UTF8;
    }


    public ArraySegment<byte> Encode(string s)
    {
      var bytesNeeded = _encoding.GetByteCount(s);
      if (bytesNeeded <= _buffer.Length) {
        var bytesWrote = _encoding.GetBytes(s, 0, s.Length, _buffer, 0);

        return new ArraySegment<byte>(_buffer, 0, bytesWrote);
      }

      var buffer = _encoding.GetBytes(s);
      return new ArraySegment<byte>(buffer, 0, buffer.Length);
    }

    public string DecodeStr(ArraySegment<byte> buffer)
    {
      return _encoding.GetString(buffer.Array, buffer.Offset, buffer.Count);
    }


    public string Encode(DateTime dt)
    {
      return (dt - UnixEpoch).TotalSeconds.ToString();
    }

    public DateTime DecodeDateTime(long dateValue)
    {
      return new DateTime(dateValue + UnixEpochL);
    }


    public string Encode(TimeSpan t)
    {
      return Settings.CultureOverride == null ?
        t.TotalSeconds.ToString() :
        t.TotalSeconds.ToString(Settings.CultureOverride);
    }

    public TimeSpan DecodeTimeSpan(long value)
    {
      return TimeSpan.FromSeconds(value);
    }


    public string Encode(double d)
    {
      if (double.IsPositiveInfinity(d)) return "+inf";
      if (double.IsNegativeInfinity(d)) return "-inf";

      return Settings.CultureOverride == null ?
        d.ToString("R") :
        d.ToString("R", Settings.CultureOverride);
    }

    public double DecodeDouble(ArraySegment<byte> buffer)
    {
      var str = Encoding.Default.GetString(buffer.Array, buffer.Offset, buffer.Count);

      if (str == "inf" || str == "+inf") return double.PositiveInfinity;
      if (str == "-inf") return double.NegativeInfinity;

      return Settings.CultureOverride == null ?
        double.Parse(str) :
        double.Parse(str, Settings.CultureOverride);
    }
  }
}
