using System;
using Sider.Tests;

namespace Sider.Tests {
  public class TestException : Exception {
    public TestException() : base() {
    }

    public TestException(string message) : base(message) {
    }

    public TestException(string message, Exception exception) : base(message, exception) {      
    }
  }
}

