
using System;
using System.IO;

namespace Sider
{
  public partial class RedisClient
  {
    public bool Ping()
    {
      _writer.WriteLine("PING");
      var result = _reader.ReadSingleLine();

      return result.Type == ResponseType.SingleLine &&
        result.Result == "PONG";
    }

    public IAsyncResult BeginPing(AsyncCallback callback, object asyncState)
    {
      var result = new SiderAsyncResult<bool>(asyncState);

      _writer.WriteLine("PING");
      _reader.BeginReadSingleLine(ar =>
      {
        var readResult = _reader.EndReadSingleLine(ar);
        result.SetResult(
          readResult.Type == ResponseType.SingleLine &&
          readResult.Result == "PONG");

        callback(result);
      }, null);

      return result;
    }

    public bool EndPing(IAsyncResult ar)
    {
      return ((SiderAsyncResult<bool>)ar).Result;
    }
  }
}
