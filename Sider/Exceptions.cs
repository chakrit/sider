using System;
using System.Runtime.Serialization;

namespace Sider {
  [DataContract]
  public class SiderException : Exception {
    public SiderException() {
    }

    public SiderException(string message)
      : base(message) {
    }

    public SiderException(string message, Exception innerException)
      : base(message, innerException) {
    }
  }

  [DataContract]
  public class ProtocolException : SiderException {
    public ProtocolException() {
    }

    public ProtocolException(string expectedValue)
      : base("expected: " + expectedValue) {
    }

    public ProtocolException(string expectedValue, params char[] actualChars)
      : this(expectedValue, new string(actualChars)) {
    }

    public ProtocolException(string expectedValue, string actualValue)
      : base("expected: " + expectedValue + " found instead: " + actualValue) {
    }
  }

  [DataContract]
  public class ConcurrentAccessException : SiderException {
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

