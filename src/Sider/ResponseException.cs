
using System;

namespace Sider
{
  public class ResponseException : Exception
  {
    public ResponseException(string msg) :
      base(msg) { }

    public ResponseException(string msg, Exception ex) :
      base(msg, ex) { }
  }
}
