
using System;

namespace Sider
{
  public sealed class WriteException : Exception
  {
    public WriteException(Exception innerException) :
      base("Exception occurred while writing data to Redis.", innerException) { }
  }

  public sealed class ReadException : Exception
  {
    public ReadException(Exception innerException) :
      base("Exception occurred while reading data from Redis.", innerException) { }
  }

  public sealed class ResponseException : Exception
  {
    public ResponseException(string msg) :
      base(msg) { }

    public ResponseException(string msg, Exception ex) :
      base(msg, ex) { }
  }
}
