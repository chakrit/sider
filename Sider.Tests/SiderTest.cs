using System;

namespace Sider.Tests {
  public abstract class SiderTest {
    static readonly Random _random = new Random();

    protected string RandomString() {
      return Guid.NewGuid().ToString();
    }

    protected int RandomInt() {
      return _random.Next();
    }

    protected Invocation<object> BuildInvocation() {
      return new Invocation<object>(
        w => {
        },
        r => null
      );
    }
  }
}

