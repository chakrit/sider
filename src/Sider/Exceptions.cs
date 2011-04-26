
using System;

namespace Sider
{
  public sealed class IdleTimeoutException : TimeoutException
  {
    public IdleTimeoutException(Exception inner) :
      base("Disconnection detected, possibly due to idle timeout.", inner) { }
  }

  public sealed class ResponseException : Exception
  {
    public ResponseException(string msg) :
      base(msg) { }

    public ResponseException(string msg, Exception ex) :
      base(msg, ex) { }
  }
}
