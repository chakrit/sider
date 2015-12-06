using System;
using System.Runtime.Serialization;

namespace Sider {
  [DataContract]
  public class ConcurrentAccessException : Exception {
    public ConcurrentAccessException() {
    }

    public ConcurrentAccessException(string message)
      : base(message) {
    }

    public ConcurrentAccessException(string message, Exception innerException)
      : base(message, innerException) {
    }
  }
}

