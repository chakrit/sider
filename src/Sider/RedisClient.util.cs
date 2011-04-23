
using System;
using System.Text;

namespace Sider
{
  public partial class RedisClient<T>
  {
    public const long UnixEpochL = 621355968000000000L; // 1st Jan 1970
    public static readonly DateTime UnixEpoch = new DateTime(UnixEpochL);


    private ArraySegment<byte> encodeStr(string s)
    {
      var bytesNeeded = Encoding.UTF8.GetByteCount(s);
      if (bytesNeeded <= _stringBuffer.Length) {
        var bytesWrote = Encoding.UTF8
          .GetBytes(s, 0, s.Length, _stringBuffer, 0);

        return new ArraySegment<byte>(_stringBuffer, 0, bytesWrote);
      }

      var buffer = Encoding.UTF8.GetBytes(s);
      return new ArraySegment<byte>(buffer, 0, buffer.Length);
    }

    // TODO: Instead of parsing raw byte[], have the reader reads
    //   into the shared _stringBuffer instead?
    private static string decodeStr(byte[] raw)
    {
      return Encoding.UTF8.GetString(raw);
    }


    private static string formatDateTime(DateTime dt)
    {
      return (dt - UnixEpoch).TotalSeconds.ToString();
    }

    private static DateTime parseDateTime(long dateValue)
    {
      return new DateTime(dateValue + UnixEpochL);
    }


    private static string formatTimeSpan(TimeSpan t)
    {
      return t.TotalSeconds.ToString();
    }

    private static TimeSpan parseTimeSpan(byte[] raw)
    {
      var str = Encoding.Default.GetString(raw);

      return TimeSpan.FromSeconds(long.Parse(str));
    }


    private string formatDbl(double d)
    {
      if (double.IsPositiveInfinity(d)) return "+inf";
      if (double.IsNegativeInfinity(d)) return "-inf";

      return _settings.CultureOverride == null ?
        d.ToString("R") :
        d.ToString("R", _settings.CultureOverride);
    }

    private double parseDouble(byte[] raw)
    {
      var str = Encoding.Default.GetString(raw);

      if (str == "inf" || str == "+inf") return double.PositiveInfinity;
      if (str == "-inf") return double.NegativeInfinity;

      return _settings.CultureOverride == null ?
        double.Parse(str) :
        double.Parse(str, _settings.CultureOverride);
    }
  }
}
