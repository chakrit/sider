using System;
using System.Dynamic;
using System.Threading.Tasks;
using System.IO;

namespace Sider.Tests {
  public abstract class SiderTestBase {
    static readonly Random _random = new Random();

    protected string RandomString() {
      return Guid.NewGuid().ToString();
    }

    protected int RandomInt() {
      return _random.Next();
    }

    protected RedisSettings RandomSettings() {
      return RedisSettings.Default.Build()
        .Host(RandomString())
        .Port(RandomInt())
        .Build();
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

