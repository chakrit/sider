using System;

namespace Sider.Tests {
  public abstract class SiderTestBase {
    static readonly Random _random = new Random();

    protected string RandomString() {
      return Guid.NewGuid().ToString();
    }

    protected int RandomInt() {
      return _random.Next();
    }
  }
}

