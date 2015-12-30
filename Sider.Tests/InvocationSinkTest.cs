using System;
using NUnit.Framework;
using System.IO;

namespace Sider.Tests {
  [TestFixture]
  public class InvocationSinkTest : SiderTest {
    [Test]
    public void TestCtor() {
      Assert.Throws<ArgumentNullException>(() => new InvocationSink(null));
    }

    [Test]
    public void TestQueue() {
      var spy = new Spy();
      var inv = new Invocation<object>(
                  spy.Action<RedisWriter>(),
                  null
                );

      var stream = new MemoryStream();
      var sink = new InvocationSink(stream);

      sink.Queue(inv);
      Assert.IsTrue(spy.Called);
      Assert.IsInstanceOf<RedisWriter>(spy.CalledArgs[0]);
    }
  }
}

