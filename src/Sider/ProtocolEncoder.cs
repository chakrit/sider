
using System;
using System.Text;

namespace Sider
{
  public class ProtocolEncoder : SettingsWrapper
  {
    protected const long UnixEpochL = 621355968000000000L; // 1st Jan 1970
    protected static readonly DateTime UnixEpoch = new DateTime(UnixEpochL);


    private byte[] _buffer;

    public byte[] SharedBuffer { get; private set; }

    public ProtocolEncoder(RedisSettings settings) :
      base(settings)
    {
      _buffer = new byte[settings.EncodingBufferSize];
    }


    public ArraySegment<byte> Encode(string s)
    {
      var bytesNeeded = Encoding.UTF8.GetByteCount(s);
      if (bytesNeeded <= _buffer.Length) {
        var bytesWrote = Encoding.UTF8
          .GetBytes(s, 0, s.Length, _buffer, 0);

        return new ArraySegment<byte>(_buffer, 0, bytesWrote);
      }

      var buffer = Encoding.UTF8.GetBytes(s);
      return new ArraySegment<byte>(buffer, 0, buffer.Length);
    }

    public string DecodeStr(ArraySegment<byte> buffer)
    {
      return Encoding.UTF8.GetString(buffer.Array, buffer.Offset, buffer.Count);
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

    public TimeSpan DecodeTimeSpan(byte[] raw)
    {
      var str = Encoding.Default.GetString(raw);
      var seconds = Settings.CultureOverride == null ?
        long.Parse(str) :
        long.Parse(str, Settings.CultureOverride);

      return TimeSpan.FromSeconds(seconds);
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
