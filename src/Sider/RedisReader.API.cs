
using System;

namespace Sider
{
  internal partial class RedisReader
  {
    public RedisResponse<string> ReadSingleLine()
    {
      RedisResponse<string> result = default(RedisResponse<string>);

      readTypeChar(fillBuffer, type =>
        readSingleLine(fillBuffer, line =>
          result = RedisResponse.Create(type, line)));

      return result;
    }

    public IAsyncResult BeginReadSingleLine(AsyncCallback callback, object asyncState)
    {
      var result = new SiderAsyncResult<RedisResponse<string>>(asyncState);

      readTypeChar(asyncFillBuffer, type =>
        readSingleLine(asyncFillBuffer, line =>
        {
          result.SetResult(RedisResponse.Create(type, line));
          callback(result);
        }));

      return result;
    }

    public RedisResponse<string> EndReadSingleLine(IAsyncResult ar)
    {
      return ((SiderAsyncResult<RedisResponse<string>>)ar).Result;
    }
  }
}
