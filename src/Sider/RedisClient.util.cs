
using System;
using System.Text;

namespace Sider
{
  public partial class RedisClient
  {
    public const long UnixEpochL = 621355968000000000L; // 1st Jan 1970
    public static readonly DateTime UnixEpoch = new DateTime(UnixEpochL);


    // TODO: use a shared/reusable buffer?
    private static byte[] encodeStr(string s)
    {
      return Encoding.UTF8.GetBytes(s);
    }

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


    private static string formatDouble(double d)
    {
      return double.IsPositiveInfinity(d) ? "+inf" :
        double.IsNegativeInfinity(d) ? "-inf" :
        d.ToString("0.0");
    }

    private static double parseDouble(byte[] raw)
    {
      var str = Encoding.Default.GetString(raw);

      return str == "inf" || str == "+inf" ? double.PositiveInfinity :
        str == "-inf" ? double.NegativeInfinity :
        double.Parse(str);
    }
  }
}
