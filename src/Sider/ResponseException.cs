
using System;
using System.Diagnostics;

namespace Sider
{
  public class ResponseException : Exception
  {
    public ResponseException(string msg) :
      base(msg) { }

    public ResponseException(string msg, Exception ex) :
      base(msg, ex) { }


    [Conditional("DEBUG")]
    public static void ExpectType(ResponseType expected, ResponseType actual)
    {
      if (expected != actual)
        throw new ResponseException(
          "Expected a `{0}` reply, got instead `{1}` reply.".F(expected, actual));
    }
  }
}
